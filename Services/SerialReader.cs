using System;
using System.IO.Ports;

namespace SerialVolumeControl.Services
{
    public class SerialReader
    {
        private SerialPort? _port;

        public event Action<int>? PotentiometerChanged;

        public void Connect(string portName, int baudRate = 9600)
        {
            _port = new SerialPort(portName, baudRate);
            _port.DataReceived += (s, e) =>
            {
                try
                {
                    var data = _port.ReadLine().Trim();
                    if (int.TryParse(data, out var value))
                    {
                        PotentiometerChanged?.Invoke(value);
                    }
                }
                catch { }
            };
            _port.Open();
        }
    }
}
