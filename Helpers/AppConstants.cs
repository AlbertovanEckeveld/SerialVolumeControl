using System;
using System.IO;

namespace SerialVolumeControl.Helpers
{
    public static class AppConstants
    {
        public const string MasterVolumeOption = "[Master Volume]";
        public const string FocusedAppOption = "[Focused Application]";
        public const string ScreenBrightnessOption = "[Screen Brightness]";

        public static string SettingsDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SerialVolumeControl");

        public static string SettingsFile =>
            Path.Combine(SettingsDirectory, "user_settings.json");
    }
}