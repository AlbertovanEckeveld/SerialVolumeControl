using System;

using System.IO;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Styling;
using Avalonia.Controls;
using Avalonia.Threading;
using SerialVolumeControl.Services;
using Avalonia.Controls.Primitives;

using SerialVolumeControl.Models;
using SerialVolumeControl.Helpers;


namespace SerialVolumeControl
{
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

        private ToggleButton? _themeToggle;

        private bool _isDarkMode = false;

        private const string FocusedAppOption = "[Focused Application]";
        private const string MasterVolumeOption = "[Master Volume]";

        public MainWindow()
        {
            InitializeComponent();

            LoadSettings();

            _themeToggle = this.FindControl<ToggleButton>("ThemeToggleButton");

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

            var portComboBox = this.FindControl<ComboBox>("PortComboBox");
            var connectButton = this.FindControl<Button>("ConnectButton");
            var disconnectButton = this.FindControl<Button>("DisconnectButton");

            if (portComboBox != null)
                portComboBox.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();

            if (connectButton != null)
                connectButton.Click += (_, _) => Connect();

            if (disconnectButton != null)
                disconnectButton.Click += (_, _) => Disconnect();

            // Build the dropdown list with extra options
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
                .Select(p => p!.ProcessName!)
                .ToList();

            // Add the special options at the top
            var dropdownOptions = new List<string> { FocusedAppOption, MasterVolumeOption };
            dropdownOptions.AddRange(processes);

            foreach (var comboBox in _appComboBoxes)
            {
                comboBox.ItemsSource = dropdownOptions;
                comboBox.Classes.Add("app-combobox");
            }

            for (int i = 0; i < Math.Min(_sliderAppAssignmentsList.Count, _appComboBoxes.Count); i++)
            {
                var appName = _sliderAppAssignmentsList[i];
                var itemsSource = _appComboBoxes[i].ItemsSource as IEnumerable<string>;
                if (!string.IsNullOrEmpty(appName) && itemsSource != null && itemsSource.Contains(appName))
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

                _volumeSliders[i].Classes.Add("app-slider");
            }

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
                        float currentVolume = 0;
                        if (selectedApp == MasterVolumeOption)
                        {
                            currentVolume = VolumeService.GetMasterVolume();
                        }
                        else if (selectedApp == FocusedAppOption)
                        {
                            var focusedApp = AppAssignmentHelper.GetForegroundProcessName();
                            currentVolume = !string.IsNullOrEmpty(focusedApp)
                                ? VolumeService.GetAppVolume(focusedApp)
                                : 0;
                        }
                        else
                        {
                            currentVolume = _savedAppVolumes.TryGetValue(selectedApp, out var savedVol)
                                ? savedVol
                                : VolumeService.GetAppVolume(selectedApp);
                        }
                        _volumeSliders[index].Value = currentVolume * 100;
                    }
                    else
                    {
                        _volumeSliders[index].Value = 0;
                    }
                    SaveSettings();
                };

                _volumeSliders[index].PropertyChanged += (_, e) =>
                {
                    if (e.Property.Name == "Value")
                    {
                        var selectedApp = _appComboBoxes[index].SelectedItem as string;
                        if (!string.IsNullOrEmpty(selectedApp))
                        {
                            float vol = (float)(_volumeSliders[index].Value / 100.0);
                            if (selectedApp == MasterVolumeOption)
                            {
                                VolumeService.SetMasterVolume(vol);
                            }
                            else if (selectedApp == FocusedAppOption)
                            {
                                var focusedApp = AppAssignmentHelper.GetForegroundProcessName();
                                if (!string.IsNullOrEmpty(focusedApp))
                                    VolumeService.SetAppVolume(focusedApp, vol);
                            }
                            else
                            {
                                VolumeService.SetAppVolume(selectedApp, vol);
                                _savedAppVolumes[selectedApp] = vol;
                            }
                            SaveSettings();
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

            if (!string.IsNullOrEmpty(_lastComPort) && System.IO.Ports.SerialPort.GetPortNames().Contains(_lastComPort))
            {
                if (portComboBox != null)
                    portComboBox.SelectedItem = _lastComPort;
                Connect();
            }
            else
            {
                if (portComboBox != null) portComboBox.IsEnabled = true;
                if (connectButton != null) connectButton.IsEnabled = true;
            }

            SetupThemeToggle(); // Call this after _themeToggle is assigned

            SetTheme(_isDarkMode);
        }

        private void Connect()
        {
            try
            {
                var portComboBox = this.FindControl<ComboBox>("PortComboBox");
                var connectButton = this.FindControl<Button>("ConnectButton");
                var disconnectButton = this.FindControl<Button>("DisconnectButton");

                if (portComboBox?.SelectedItem is string portName)
                {
                    _reader.Connect(portName);

                    if (connectButton != null) connectButton.IsEnabled = false;
                    if (portComboBox != null) portComboBox.IsEnabled = false;
                    if (disconnectButton != null) disconnectButton.IsEnabled = true;

                    _lastComPort = portName;
                    SaveSettings();
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
                var portComboBox = this.FindControl<ComboBox>("PortComboBox");
                var connectButton = this.FindControl<Button>("ConnectButton");
                var disconnectButton = this.FindControl<Button>("DisconnectButton");

                _reader.Disconnect();

                if (connectButton != null) connectButton.IsEnabled = true;
                if (portComboBox != null) portComboBox.IsEnabled = true;
                if (disconnectButton != null) disconnectButton.IsEnabled = false;

                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from serial port: {ex}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new UserSettings
                {
                    LastComPort = _lastComPort,
                    AppVolumes = _savedAppVolumes,
                    SliderAppAssignments = _sliderAppAssignmentsList,
                    IsDarkMode = _isDarkMode // Save theme setting
                };
                File.WriteAllText(SettingsFile, JsonSerializer.Serialize(settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<UserSettings>(json);

                    if (settings != null)
                    {
                        _lastComPort = settings.LastComPort;
                        _savedAppVolumes = settings.AppVolumes ?? new Dictionary<string, float>();
                        _sliderAppAssignmentsList = settings.SliderAppAssignments ?? new List<string?> { null, null, null, null, null };
                        _isDarkMode = settings.IsDarkMode; // Load theme setting
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex}");
            }
        }

        private void SetTheme(bool isDark)
        {
            _isDarkMode = isDark;
            Application.Current!.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
            SaveSettings();
        }

        private void SetupThemeToggle()
        {
            if (_themeToggle != null)
            {
                _themeToggle.IsChecked = _isDarkMode;
                _themeToggle.IsCheckedChanged += (_, _) =>
                {
                    SetTheme(_themeToggle.IsChecked == true);
                };
            }
        }

    }
}
