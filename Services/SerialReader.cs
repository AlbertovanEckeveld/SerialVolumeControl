using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SerialVolumeControl.Services
{
    public class SerialReader
    {
        private SerialPort? _port;
        private readonly StringBuilder _lineBuffer = new();
        private readonly ConcurrentQueue<string> _receivedLines = new();
        private Timer? _processTimer;

        public event Action<int, int>? SliderChanged;

        // Connect method to initialize the serial port and start listening for data
        public void Connect(string portName, int baudRate = 9600)
        {
            _port = new SerialPort(portName, baudRate);
            _port.DataReceived += OnDataReceived;
            _port.Open();

            // Start a timer to process lines on the main thread every 20ms
            _processTimer = new Timer(ProcessLines, null, 0, 20);
        }

        // Event handler for data received from the serial port
        private void OnDataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (_port != null && _port.BytesToRead > 0)
                {
                    char c = (char)_port.ReadChar();
                    if (c == '\n' || c == '\r')
                    {
                        if (_lineBuffer.Length > 0)
                        {
                            _receivedLines.Enqueue(_lineBuffer.ToString());
                            _lineBuffer.Clear();
                        }
                    }
                    else
                    {
                        _lineBuffer.Append(c);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial read error: {ex}");
            }
        }

        // Process lines from the queue on the main thread
        private void ProcessLines(object? state)
        {
            while (_receivedLines.TryDequeue(out var line))
            {
                var data = line.Trim();
                var match = Regex.Match(data, @"slider(\d+):\s*(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int sliderIndex = int.Parse(match.Groups[1].Value);
                    int value = int.Parse(match.Groups[2].Value);
                    SliderChanged?.Invoke(sliderIndex, value);
                }
            }
        }

        // Disconnect method to close the serial port and dispose of resources
        public void Disconnect()
        {
            if (_port != null)
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
                _port.Dispose();
                _port = null;
            }
            _processTimer?.Dispose();
            _processTimer = null;
        }

        public bool IsConnected => _port?.IsOpen == true;
    }
}
