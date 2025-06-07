using System;
using System.Linq;
using System.Collections.Generic;

using Avalonia.Controls;

using SerialVolumeControl.Helpers;
using SerialVolumeControl.Services;


namespace SerialVolumeControl
{
    public partial class MainWindow
    {
        /// <summary>
        /// Initializes the ComboBoxes and Sliders used for volume control,
        /// and adds them to their respective internal lists.
        /// </summary>
        /// <remarks>
        /// - Finds controls named "AppComboBox1" to "AppComboBox5" and adds them to <c>_appComboBoxes</c>.
        /// - Finds controls named "VolumeSlider1" to "VolumeSlider5" and adds them to <c>_volumeSliders</c>.
        /// </remarks>
        private void InitializeComboBoxesAndSliders()
        {
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
        }

        /// <summary>
        /// Initializes the serial port ComboBox and connect/disconnect buttons,
        /// and binds event handlers to their click events.
        /// </summary>
        /// <remarks>
        /// - Populates the "PortComboBox" with available COM port names.
        /// - Sets up click handlers for "ConnectButton" and "DisconnectButton".
        /// </remarks>
        private void InitializePortControls()
        {
            var portComboBox = this.FindControl<ComboBox>("PortComboBox");
            var connectButton = this.FindControl<Button>("ConnectButton");
            var disconnectButton = this.FindControl<Button>("DisconnectButton");

            if (portComboBox != null)
                portComboBox.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();

            if (connectButton != null)
                connectButton.Click += (_, _) => Connect();

            if (disconnectButton != null)
                disconnectButton.Click += (_, _) => Disconnect();

            if (!string.IsNullOrEmpty(_lastComPort) && portComboBox != null)
            {
                var portNames = System.IO.Ports.SerialPort.GetPortNames();
                if (portNames.Contains(_lastComPort))
                {
                    portComboBox.SelectedItem = _lastComPort;
                    Connect();
                }
                else
                {
                    portComboBox.IsEnabled = true;
                    if (connectButton != null) connectButton.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Populates all application ComboBoxes with process options,
        /// including special entries for focused app and master volume.
        /// </summary>
        /// <remarks>
        /// - Uses <see cref="ProcessHelper.GetProcessNames"/> to get the list of running processes.
        /// - Adds predefined options from <see cref="AppConstants"/>.
        /// - Applies the CSS class <c>app-combobox</c> to each ComboBox.
        /// </remarks>
        private void InitializeDropdowns()
        {
            var processes = ProcessHelper.GetProcessNames();
            var dropdownOptions = new List<string> { 
                AppConstants.FocusedAppOption, 
                AppConstants.MasterVolumeOption,
                AppConstants.ScreenBrightnessOption
            };
            dropdownOptions.AddRange(processes);

            foreach (var comboBox in _appComboBoxes)
            {
                comboBox.ItemsSource = dropdownOptions;
                comboBox.Classes.Add("app-combobox");
            }
        }

        /// <summary>
        /// Assigns applications to sliders based on saved assignments, sets their initial volumes,
        /// and attaches change listeners to handle user interactions.
        /// </summary>
        /// <remarks>
        /// - Matches saved app assignments to UI elements.
        /// - Sets initial volume levels using <see cref="VolumeService"/>.
        /// - Updates volume when ComboBox selection or Slider value changes.
        /// - Supports "Master Volume", "Focused App", and regular process-based volume control.
        /// - Persists changes to saved settings.
        /// </remarks>
        private void InitializeSliderAssignments()
        {
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
                        else if (selectedApp == AppConstants.ScreenBrightnessOption)
                        {
                            _volumeSliders[index].Value = ScreenBrightnessService.GetBrightness();
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
                            if (selectedApp == AppConstants.MasterVolumeOption)
                            {
                                float vol = (float)(_volumeSliders[index].Value / 100.0);
                                VolumeService.SetMasterVolume(vol);
                            }
                            else if (selectedApp == AppConstants.FocusedAppOption)
                            {
                                float vol = (float)(_volumeSliders[index].Value / 100.0);
                                var focusedApp = AppAssignmentHelper.GetForegroundProcessName();
                                if (!string.IsNullOrEmpty(focusedApp))
                                    VolumeService.SetAppVolume(focusedApp, vol);
                            }
                            else if (selectedApp == AppConstants.ScreenBrightnessOption)
                            {
                                ScreenBrightnessService.SetBrightness((int)_volumeSliders[index].Value);
                            }
                            else
                            {
                                float vol = (float)(_volumeSliders[index].Value / 100.0);
                                VolumeService.SetAppVolume(selectedApp, vol);
                                _savedAppVolumes[selectedApp] = vol;
                            }
                            SaveSettings();
                        }
                    }
                };

                _sliderAppAssignments[index] = null;
            }
        }
    }
}