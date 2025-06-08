using System;
using System.IO;

namespace SerialVolumeControl.Helpers
{
    /// <summary>
    /// Provides constant values and utility paths used throughout the SerialVolumeControl application.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Option string representing control of the master system volume.
        /// </summary>
        public const string MasterVolumeOption = "[Master Volume]";

        /// <summary>
        /// Option string representing control of the currently focused application's volume.
        /// </summary>
        public const string FocusedAppOption = "[Focused Application]";

        /// <summary>
        /// Option string representing control of the screen brightness.
        /// </summary>
        public const string ScreenBrightnessOption = "[Screen Brightness]";

        /// <summary>
        /// Gets the full path to the directory where application settings are stored.
        /// Located in the user's Application Data folder.
        /// </summary>
        public static string SettingsDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SerialVolumeControl");

        /// <summary>
        /// Gets the full path to the user settings file (user_settings.json) used by the application.
        /// </summary>
        public static string SettingsFile =>
            Path.Combine(SettingsDirectory, "user_settings.json");
    }
}