using System;
using System.Management;
using System.Runtime.InteropServices;

namespace SerialVolumeControl.Helpers
{
    public static class ScreenBrightnessHelper
    {
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