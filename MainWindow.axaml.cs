using Avalonia.Controls;
using Avalonia.Threading;
using SerialVolumeControl.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SerialVolumeControl;

public partial class MainWindow : Window
{
    private readonly List<ComboBox> _appComboBoxes = new();
    private readonly List<Slider> _volumeSliders = new();
    private readonly Dictionary<int, string?> _sliderAppAssignments = new();
    private readonly SerialReader _reader = new();

    public MainWindow()
    {
        InitializeComponent();

        PortComboBox.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
        ConnectButton.Click += (_, _) => Connect();
        DisconnectButton.Click += (_, _) => Disconnect();

        foreach (var name in new[] { "AppComboBox1", "AppComboBox2", "AppComboBox3", "AppComboBox4", "AppComboBox5" })
        {
            var combo = this.FindControl<ComboBox>(name);
            if (combo != null) _appComboBoxes.Add(combo);
        }

        foreach (var name in new[] { "VolumeSlider1", "VolumeSlider2", "VolumeSlider3", "VolumeSlider4", "VolumeSlider5" })
        {
            var slider = this.FindControl<Slider>(name);
            if (slider != null) _volumeSliders.Add(slider);
        }

        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
            .Select(p =>
            {
                try { return new { p.ProcessName, p.MainWindowTitle, Path = p.MainModule.FileName }; }
                catch { return null; }
            })
            .Where(p => p != null)
            .DistinctBy(p => p.Path)
            .OrderBy(p => p.ProcessName)
            .ToList();

        foreach (var comboBox in _appComboBoxes)
        {
            comboBox.ItemsSource = processes.Select(p => p.ProcessName).ToList();
        }

        for (int i = 0; i < Math.Min(_volumeSliders.Count, _appComboBoxes.Count); i++)
        {
            int index = i;

            _appComboBoxes[index].SelectionChanged += (sender, e) =>
            {
                var selectedApp = _appComboBoxes[index].SelectedItem as string;
                _sliderAppAssignments[index] = selectedApp;

                if (!string.IsNullOrEmpty(selectedApp))
                {
                    float currentVolume = VolumeService.GetAppVolume(selectedApp);
                    _volumeSliders[index].Value = currentVolume * 100;
                }
                else
                {
                    _volumeSliders[index].Value = 0;
                }
            };

            _volumeSliders[index].PropertyChanged += (_, e) =>
            {
                if (e.Property.Name == "Value")
                {
                    var selectedApp = _appComboBoxes[index].SelectedItem as string;
                    if (!string.IsNullOrEmpty(selectedApp))
                    {
                        float vol = (float)(_volumeSliders[index].Value / 100.0);
                        VolumeService.SetAppVolume(selectedApp, vol);
                    }
                }
            };

            _sliderAppAssignments[index] = null;
        }

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

    private void Connect()
    {
        try
        {
            if (PortComboBox.SelectedItem is string portName)
            {
                _reader.Connect(portName);
                ConnectButton.IsEnabled = false;
                PortComboBox.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to serial port: {ex}");
        }
    }

    private void Disconnect()
    {
        try
        {
            _reader.Disconnect();
            ConnectButton.IsEnabled = true;
            PortComboBox.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disconnecting from serial port: {ex}");
        }
    }
}