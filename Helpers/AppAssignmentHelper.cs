using System.Diagnostics;

namespace SerialVolumeControl.Helpers;

/// <summary>
/// Provides helper methods related to process and window assignment.
/// </summary>
public static class AppAssignmentHelper
{
    /// <summary>
    /// Retrieves the process name of the application that currently has focus (is in the foreground).
    /// </summary>
    /// <returns>
    /// The name of the foreground process, or <c>null</c> if the process could not be determined.
    /// </returns>
    /// <remarks>
    /// This method uses native Windows API calls to identify the process associated with the active window.
    /// Useful for context-aware volume control or monitoring active applications.
    /// </remarks>
    public static string? GetForegroundProcessName()
    {
        try
        {
            var hwnd = NativeMethods.GetForegroundWindow();
            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            var proc = Process.GetProcessById((int)pid);
            return proc.ProcessName;
        }
        catch
        {
            return null;
        }
    }
}
