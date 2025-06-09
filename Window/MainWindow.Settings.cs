
using SerialVolumeControl.Helpers;
using SerialVolumeControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;


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
        /// <summary>
        /// Enables or disables automatic startup of the application when the system starts,
        /// depending on the operating system (Windows, Linux, or macOS).
        /// </summary>
        /// <param name="enable">
        /// If <c>true</c>, configures the system to start the application on login/startup;
        /// if <c>false</c>, removes the auto-start configuration.
        /// </param>
        /// <remarks>
        /// - **Windows**: Creates or removes a shortcut (.lnk) in the user's Startup folder.
        /// - **Linux**: Writes or deletes a .desktop file in ~/.config/autostart.
        /// - **macOS**: Creates or removes a LaunchAgent .plist file in ~/Library/LaunchAgents.
        ///
        /// Errors during file or shortcut creation are caught and logged to the console.
        /// </remarks>
        /// <example>
        /// Enable auto-start:
        /// <code>
        /// SetAutoStart(true);
        /// </code>
        /// Disable auto-start:
        /// <code>
        /// SetAutoStart(false);
        /// </code>
        /// </example>
        /// <exception cref="Exception">
        /// Thrown if WScript.Shell is not available on Windows when trying to create the shortcut.
        /// </exception>
        public static void SetAutoStart(bool enable)
        {
            string exePath = Process.GetCurrentProcess().MainModule!.FileName!; 
            string appName = "SerialVolumeControl";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, $"{appName}.lnk");

                if (enable)
                {
                    try
                    {
                        Type? t = Type.GetTypeFromProgID("WScript.Shell");
                        if (t == null) throw new Exception("WScript.Shell not available");
                        dynamic shell = Activator.CreateInstance(t)!;
                        var shortcut = shell.CreateShortcut(shortcutPath);
                        shortcut.TargetPath = exePath;
                        shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                        shortcut.Save();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create Windows startup shortcut: {ex}");
                    }
                }
                else if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string autostartDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config", "autostart");
                Directory.CreateDirectory(autostartDir);
                string desktopFile = Path.Combine(autostartDir, $"{appName}.desktop");

                if (enable)
                {
                    string content = $@"[Desktop Entry]
Type=Application
Exec=""{exePath}""
Hidden=false
NoDisplay=false
X-GNOME-Autostart-enabled=true
Name={appName}
Comment=Start {appName} at login
";
                    File.WriteAllText(desktopFile, content);
                }
                else if (File.Exists(desktopFile))
                {
                    File.Delete(desktopFile);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string launchAgentsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "LaunchAgents");
                Directory.CreateDirectory(launchAgentsDir);
                string plistPath = Path.Combine(launchAgentsDir, $"com.{appName.ToLower()}.plist");

                if (enable)
                {
                    string plist = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.{appName.ToLower()}</string>
    <key>ProgramArguments</key>
    <array>
        <string>{exePath}</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>
";
                    File.WriteAllText(plistPath, plist);
                }
                else if (File.Exists(plistPath))
                {
                    File.Delete(plistPath);
                }
            }
        }

    }
}
