using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SerialVolumeControl.Helpers
{
    /// <summary>
    /// Provides utility functions for retrieving and filtering running system processes.
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Retrieves a list of distinct process names for applications with a visible main window.
        /// </summary>
        /// <returns>
        /// A list of unique process names sorted alphabetically. Only processes with a non-empty
        /// <see cref="Process.MainWindowTitle"/> and accessible <see cref="Process.MainModule"/> are included.
        /// </returns>
        /// <remarks>
        /// Processes without a main window or with inaccessible module information (due to permissions)
        /// are ignored. The result list is deduplicated based on the full path of the process executable.
        /// </remarks>
        public static List<string> GetProcessNames()
        {
            return Process.GetProcesses()
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
        }
    }
}
