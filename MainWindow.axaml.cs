using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Avalonia.Controls;
using Avalonia.Threading;
using SerialVolumeControl.Services;
using Avalonia.Controls.Primitives;

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

        private List<string?> _sliderAppAssignmentsList = new() { null, null, null, null, null };

        private ToggleButton? _themeToggle;

        private bool _isDarkMode = false;

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

            var processes = ProcessHelper.GetProcessNames();
            var dropdownOptions = new List<string> { AppConstants.FocusedAppOption, AppConstants.MasterVolumeOption };
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
                        if (selectedApp == AppConstants.MasterVolumeOption)
                        {
                            currentVolume = VolumeService.GetMasterVolume();
                        }
                        else if (selectedApp == AppConstants.FocusedAppOption)
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
                            if (selectedApp == AppConstants.MasterVolumeOption)
                            {
                                VolumeService.SetMasterVolume(vol);
                            }
                            else if (selectedApp == AppConstants.FocusedAppOption)
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

            SetupThemeToggle();
            SetTheme(_isDarkMode);
        }

    }
}
