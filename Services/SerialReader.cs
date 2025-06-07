using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SerialVolumeControl.Services
{
    /// <summary>
    /// Reads and processes serial data from a connected device, such as a volume slider.
    /// </summary>
    /// <remarks>
    /// The class connects to a serial port, reads incoming data, and raises events when valid slider messages are received.
    /// </remarks>
    public class SerialReader
    {
        private SerialPort? _port;
        private readonly StringBuilder _lineBuffer = new();
        private readonly ConcurrentQueue<string> _receivedLines = new();
        private Timer? _processTimer;

        /// <summary>
        /// Event that is triggered when a slider value changes.
        /// </summary>
        /// <remarks>
        /// The event provides the slider index and its new value.
        /// </remarks>
        public event Action<int, int>? SliderChanged;

        /// <summary>
        /// Connects to the specified serial port and starts listening for data.
        /// </summary>
        /// <param name="portName">The name of the serial port to connect to (e.g., "COM3").</param>
        /// <param name="baudRate">The baud rate for the connection. Default is 9600.</param>
        /// <example>
        /// <code>
        /// var reader = new SerialReader();
        /// reader.Connect("COM3", 115200);
        /// </code>
        /// </example>
        public void Connect(string portName, int baudRate = 9600)
        {
            _port = new SerialPort(portName, baudRate);
            _port.DataReceived += OnDataReceived;
            _port.Open();

            // Start a timer to process lines every 20ms
            _processTimer = new Timer(ProcessLines, null, 0, 20);
        }

        /// <summary>
        /// Handles incoming serial data and accumulates lines for processing.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments containing data information.</param>
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

        /// <summary>
        /// Processes received lines and triggers the <see cref="SliderChanged"/> event when a valid slider message is parsed.
        /// </summary>
        /// <param name="state">Unused timer state object.</param>
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

        /// <summary>
        /// Disconnects from the serial port and releases all resources.
        /// </summary>
        /// <example>
        /// <code>
        /// reader.Disconnect();
        /// </code>
        /// </example>
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

        /// <summary>
        /// Gets a value indicating whether the serial port is currently connected and open.
        /// </summary>
        public bool IsConnected => _port?.IsOpen == true;
    }
}
