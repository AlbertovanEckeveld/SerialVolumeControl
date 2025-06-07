
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using SerialVolumeControl.Models;
using SerialVolumeControl.Helpers;

namespace SerialVolumeControl
{
    /// <summary>
    /// Partial class containing persistence logic for user settings in the main window.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Saves user settings such as last selected COM port, volume mappings, slider assignments,
        /// and theme preference to a JSON file on disk.
        /// </summary>
        /// <remarks>
        /// This method serializes the <see cref="UserSettings"/> object to a JSON string and writes it
        /// to the file specified by <c>SettingsFile</c>. Exceptions are caught and logged to the console.
        /// </remarks>
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
                File.WriteAllText(AppConstants.SettingsFile, JsonSerializer.Serialize(settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex}");
            }
        }

        /// <summary>
        /// Loads user settings from a JSON file and restores them into the application state.
        /// </summary>
        /// <remarks>
        /// If the settings file exists, it is read and deserialized into a <see cref="UserSettings"/> object.
        /// Values such as the last used COM port, volume mappings, slider assignments, and theme preference
        /// are restored. Default values are applied when data is missing.
        /// </remarks>
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(AppConstants.SettingsFile))
                {
                    var json = File.ReadAllText(AppConstants.SettingsFile);
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
    }
}
