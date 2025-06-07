using Avalonia.Controls;
using Avalonia.Threading;
using SerialVolumeControl.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace SerialVolumeControl;

public partial class MainWindow : Window
{
    private readonly List<ComboBox> _appComboBoxes = new();
    private readonly List<Slider> _volumeSliders = new();
    private readonly Dictionary<int, string?> _sliderAppAssignments = new();
    private readonly SerialReader _reader = new();

    private Dictionary<string, float> _savedAppVolumes = new();
    private string? _lastComPort;

    private const string SettingsFile = "user_settings.json";

    private List<string?> _sliderAppAssignmentsList = new() { null, null, null, null, null };


#if WINDOWS
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
#endif

    public MainWindow()
    {
        // Initialize the main window and load settings
        InitializeComponent();

        // Set up the main window properties
        LoadSettings();

        // Initialize the serial reader
        PortComboBox.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
        ConnectButton.Click += (_, _) => Connect();
        DisconnectButton.Click += (_, _) => Disconnect();

        // Initialize combo boxes for app selection
        foreach (var name in new[] { "AppComboBox1", "AppComboBox2", "AppComboBox3", "AppComboBox4", "AppComboBox5" })
        {
            var combo = this.FindControl<ComboBox>(name);
            if (combo != null) _appComboBoxes.Add(combo);
        }

        // Initialize volume sliders
        foreach (var name in new[] { "VolumeSlider1", "VolumeSlider2", "VolumeSlider3", "VolumeSlider4", "VolumeSlider5" })
        {
            var slider = this.FindControl<Slider>(name);
            if (slider != null) _volumeSliders.Add(slider);
        }

        // Initialize slider assignments
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
            .Select(p =>
            {
                try
                {
                    var fileName = p.MainModule?.FileName;
                    if (fileName == null)
                        return null;
                    return new { p.ProcessName, p.MainWindowTitle, Path = fileName };
                }
                catch
                {
                    return null;
                }
            })
            .Where(p => p != null && p.ProcessName != null && p.Path != null)
            .DistinctBy(p => p!.Path)
            .OrderBy(p => p!.ProcessName)
            .ToList();

        // Populate combo boxes with process names
        foreach (var comboBox in _appComboBoxes)
        {
            comboBox.ItemsSource = processes
                .Where(p => p != null && p.ProcessName != null)
                .Select(p => p!.ProcessName!)
                .ToList();
        }

        // Restore saved app assignments to combo boxes
        for (int i = 0; i < Math.Min(_sliderAppAssignmentsList.Count, _appComboBoxes.Count); i++)
        {
            var appName = _sliderAppAssignmentsList[i];
            if (!string.IsNullOrEmpty(appName) && _appComboBoxes[i].Items != null && _appComboBoxes[i].Items.Contains(appName))
            {
                _appComboBoxes[i].SelectedItem = appName;
                _sliderAppAssignments[i] = appName;
                float currentVolume = _savedAppVolumes.TryGetValue(appName, out var savedVol)
                    ? savedVol
                    : VolumeService.GetAppVolume(appName);
                _volumeSliders[i].Value = currentVolume * 100;
            }
            else
            {
                _appComboBoxes[i].SelectedItem = null;
                _sliderAppAssignments[i] = null;
                _volumeSliders[i].Value = 0;
            }
        }

        // Set up event handlers for each slider and combo box
        for (int i = 0; i < Math.Min(_volumeSliders.Count, _appComboBoxes.Count); i++)
        {
            int index = i;

            _appComboBoxes[index].SelectionChanged += (sender, e) =>
            {
                var selectedApp = _appComboBoxes[index].SelectedItem as string;
                _sliderAppAssignments[index] = selectedApp;
                _sliderAppAssignmentsList[index] = selectedApp;

                if (!string.IsNullOrEmpty(selectedApp))
                {
                    float currentVolume = _savedAppVolumes.TryGetValue(selectedApp, out var savedVol)
                        ? savedVol
                        : VolumeService.GetAppVolume(selectedApp);
                    _volumeSliders[index].Value = currentVolume * 100;
                }
                else
                {
                    _volumeSliders[index].Value = 0;
                }
                SaveSettings();
            };

            // Set initial slider value based on saved volume or current app volume
            _volumeSliders[index].PropertyChanged += (_, e) =>
            {
                if (e.Property.Name == "Value")
                {
                    var selectedApp = _appComboBoxes[index].SelectedItem as string;
                    if (!string.IsNullOrEmpty(selectedApp))
                    {
                        float vol = (float)(_volumeSliders[index].Value / 100.0);
                        VolumeService.SetAppVolume(selectedApp, vol);

                        _savedAppVolumes[selectedApp] = vol;
                        SaveSettings();
                    }
                }
            };

            _sliderAppAssignments[index] = null;
        }

        // Handle slider changes from the serial reader
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

        // Try auto-connect to last COM port
        if (!string.IsNullOrEmpty(_lastComPort) && System.IO.Ports.SerialPort.GetPortNames().Contains(_lastComPort))
        {
            PortComboBox.SelectedItem = _lastComPort;
            Connect();
        }
        else
        {
            PortComboBox.IsEnabled = true;
            ConnectButton.IsEnabled = true;
        }
    }

    // Try to connect to the serial port when the window is loaded
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
                _lastComPort = portName;
                SaveSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to serial port: {ex}");
        }
    }

    // Disconnect from the serial port when the user clicks the disconnect button
    private void Disconnect()
    {
        try
        {
            _reader.Disconnect();
            ConnectButton.IsEnabled = true;
            PortComboBox.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            SaveSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disconnecting from serial port: {ex}");
        }
    }

    // Save user settings to a JSON file
    private void SaveSettings()
    {
        try
        {
            var settings = new UserSettings
            {
                LastComPort = _lastComPort,
                AppVolumes = _savedAppVolumes,
                SliderApps = _sliderAppAssignmentsList
            };
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(settings));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save settings: {ex}");
        }
    }

    // Load user settings from a JSON file
    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var settings = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(SettingsFile));
                if (settings != null)
                {
                    _lastComPort = settings.LastComPort;
                    _savedAppVolumes = settings.AppVolumes ?? new Dictionary<string, float>();
                    _sliderAppAssignmentsList = settings.SliderApps ?? new List<string?> { null, null, null, null, null };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load settings: {ex}");
        }
    }

    // Class to hold user settings for serialization
    private class UserSettings
    {
        public string? LastComPort { get; set; }
        public Dictionary<string, float>? AppVolumes { get; set; }
        public List<string?>? SliderApps { get; set; } // Add this for slider-app mapping
    }
}
