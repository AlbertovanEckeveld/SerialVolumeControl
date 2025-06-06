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
    private readonly List<ComboBox> _appComboBoxes;
    private readonly List<Slider> _volumeSliders;

    private SerialReader _reader = new SerialReader();

    public MainWindow()
    {
        InitializeComponent();

        PortComboBox.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
        ConnectButton.Click += (_, _) => Connect();

        _appComboBoxes = new List<ComboBox>
        {
            this.FindControl<ComboBox>("AppComboBox1"),
            this.FindControl<ComboBox>("AppComboBox2"),
            this.FindControl<ComboBox>("AppComboBox3"),
            this.FindControl<ComboBox>("AppComboBox4"),
            this.FindControl<ComboBox>("AppComboBox5"),
        };

        _volumeSliders = new List<Slider>
        {
            this.FindControl<Slider>("VolumeSlider1"),
            this.FindControl<Slider>("VolumeSlider2"),
            this.FindControl<Slider>("VolumeSlider3"),
            this.FindControl<Slider>("VolumeSlider4"),
            this.FindControl<Slider>("VolumeSlider5"),
        };

        // Populate ComboBoxes with running processes (apps)
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
            .Select(p => p.ProcessName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        foreach (var comboBox in _appComboBoxes)
        {
            comboBox.ItemsSource = processes;
        }

        // Attach event handlers for sliders
        for (int i = 0; i < _volumeSliders.Count; i++)
        {
            int index = i;
            _volumeSliders[i].PropertyChanged += (_, e) =>
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
        }
    }

    private void Connect()
    {
        if (PortComboBox.SelectedItem is string portName)
        {
            _reader.Connect(portName);
        }
    }
}