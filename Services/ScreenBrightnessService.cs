using System;
using System.Management;
using System.Runtime.InteropServices;

namespace SerialVolumeControl.Services
{
    public static class ScreenBrightnessService
    {
        /// <summary>
        /// Retrieves the current screen brightness percentage.
        /// </summary>
        /// <returns>
        /// An integer representing the current brightness level (0–100).
        /// Returns 100 if not running on Windows or if retrieval fails.
        /// </returns>
        public static int GetBrightness()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return 100; 

            try
            {
                using var mclass = new ManagementClass("WmiMonitorBrightness");
                mclass.Scope = new ManagementScope(@"\\.\root\wmi");
                foreach (var instance in mclass.GetInstances())
                {
                    return (byte)((ManagementObject)instance)["CurrentBrightness"];
                }
            }
            catch { }
            return 100;
        }


        /// <summary>
        /// Sets the screen brightness to the specified level.
        /// </summary>
        /// <param name="brightness">
        /// An integer between 0 and 100 representing the desired brightness level.
        /// </param>
        /// <remarks>
        /// Only works on Windows systems where WMI access to monitor brightness is available.
        /// Does nothing if called on a non-Windows platform or if an error occurs.
        /// </remarks>
        public static void SetBrightness(int brightness)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            try
            {
                using var mclass = new ManagementClass("WmiMonitorBrightnessMethods");
                mclass.Scope = new ManagementScope(@"\\.\root\wmi");
                foreach (var instance in mclass.GetInstances())
                {
                    var args = new object[] { 1, (byte)brightness };
                    ((ManagementObject)instance).InvokeMethod("WmiSetBrightness", args);
                }
            }
            catch { }
        }
    }
}