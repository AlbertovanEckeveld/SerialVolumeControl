using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Management;

namespace SerialVolumeControl.Services
{
    /// <summary>
    /// Provides methods to get and set the screen brightness on all monitors (DDC/CI for extern, WMI for intern).
    /// </summary>
    public static class ScreenBrightnessService
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left, top, right, bottom;
        }

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

        /// <summary>
        /// Gets the average brightness of all monitors (internal + external).
        /// </summary>
        public static int GetBrightness()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return 100;

            // Probeer eerst interne (WMI) helderheid
            int? wmiBrightness = TryGetInternalBrightness();
            // Probeer externe (DDC/CI) helderheid
            int? ddcBrightness = TryGetDdcBrightness();

            if (wmiBrightness.HasValue && ddcBrightness.HasValue)
                return (wmiBrightness.Value + ddcBrightness.Value) / 2;
            if (wmiBrightness.HasValue)
                return wmiBrightness.Value;
            if (ddcBrightness.HasValue)
                return ddcBrightness.Value;
            return 100;
        }

        /// <summary>
        /// Sets the brightness of all monitors (internal + external).
        /// </summary>
        public static void SetBrightness(int brightness)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            // Zet interne (laptop) schermen via WMI
            TrySetInternalBrightness(brightness);

            // Zet externe schermen via DDC/CI
            SetDdcBrightness(brightness);
        }

        // --- Interne schermen (WMI) ---
        private static int? TryGetInternalBrightness()
        {
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
            return null;
        }

        private static void TrySetInternalBrightness(int brightness)
        {
            try
            {
                using var mclass = new ManagementClass("WmiMonitorBrightnessMethods");
                mclass.Scope = new ManagementScope(@"\\.\root\wmi");
                foreach (var instance in mclass.GetInstances())
                {
                    if (instance is ManagementObject mo)
                    {
                        var args = new object[] { 1, (byte)brightness };
                        mo.InvokeMethod("WmiSetBrightness", args);
                    }
                }
            }
            catch { }
        }

        // --- Externe schermen (DDC/CI) ---
        private static int? TryGetDdcBrightness()
        {
            var allMonitors = GetAllPhysicalMonitors();
            int total = 0, count = 0;

            foreach (var mon in allMonitors)
            {
                try
                {
                    if (GetMonitorBrightness(mon.hPhysicalMonitor, out uint min, out uint current, out uint max) && max > min)
                    {
                        int percent = (int)((current - min) * 100 / (max - min));
                        total += percent;
                        count++;
                    }
                }
                catch { }
            }
            DestroyAllPhysicalMonitors(allMonitors);

            return count > 0 ? total / count : (int?)null;
        }

        private static void SetDdcBrightness(int brightness)
        {
            var allMonitors = GetAllPhysicalMonitors();
            Parallel.ForEach(allMonitors, new ParallelOptions { MaxDegreeOfParallelism = 4 }, mon =>
            {
                try
                {
                    if (GetMonitorBrightness(mon.hPhysicalMonitor, out uint min, out uint current, out uint max) && max > min)
                    {
                        uint newValue = (uint)(min + (brightness * (max - min) / 100));
                        SetMonitorBrightness(mon.hPhysicalMonitor, newValue);
                    }
                }
                catch { }
            });
            DestroyAllPhysicalMonitors(allMonitors);
        }

        private static List<PHYSICAL_MONITOR> GetAllPhysicalMonitors()
        {
            var result = new List<PHYSICAL_MONITOR>();
            MonitorEnumProc proc = (IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data) =>
            {
                try
                {
                    if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint num) && num > 0)
                    {
                        var monitors = new PHYSICAL_MONITOR[num];
                        if (GetPhysicalMonitorsFromHMONITOR(hMonitor, num, monitors))
                        {
                            result.AddRange(monitors);
                        }
                    }
                }
                catch { }
                return true;
            };
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, proc, IntPtr.Zero);
            return result;
        }

        private static void DestroyAllPhysicalMonitors(List<PHYSICAL_MONITOR> monitors)
        {
            if (monitors == null || monitors.Count == 0)
                return;
            try
            {
                var arr = monitors.ToArray();
                DestroyPhysicalMonitors((uint)arr.Length, arr);
            }
            catch { }
        }
    }
}
