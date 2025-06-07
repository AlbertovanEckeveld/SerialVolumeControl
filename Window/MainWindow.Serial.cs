using System;
using Avalonia.Threading;

namespace SerialVolumeControl
{
    public partial class MainWindow
    {
        private void InitializeSerialReader()
        {
            _reader.SliderChanged += (sliderIndex, value) =>
            {
                Console.WriteLine($"UI received: slider {sliderIndex} value={value}");
                if (sliderIndex >= 0 && sliderIndex < _volumeSliders.Count)
                {
                    double sliderValue = Math.Clamp(value / 1023.0 * 100, 0, 100);
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Console.WriteLine($"Setting slider {sliderIndex} to {sliderValue}");
                        _volumeSliders[sliderIndex].Value = sliderValue;
                    });
                }
            };
        }
    }
}