using System;
using System.Runtime.InteropServices;

namespace SerialVolumeControl.Helpers;

/// <summary>
/// Contains P/Invoke declarations for native Win32 methods used in window and process handling.
/// </summary>
public static class NativeMethods
{
    /// <summary>
    /// Retrieves a handle to the foreground window (the window with which the user is currently interacting).
    /// </summary>
    /// <returns>
    /// A handle to the foreground window. The return value is <c>IntPtr.Zero</c> if no foreground window exists.
    /// </returns>
    /// <remarks>
    /// This function can be used to determine which window is currently active.
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// Retrieves the identifier of the thread that created the specified window and optionally retrieves the process ID.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="lpdwProcessId">
    /// When the function returns, this parameter contains the process identifier of the window's creator.
    /// </param>
    /// <returns>
    /// The identifier of the thread that created the window.
    /// </returns>
    /// <remarks>
    /// This function is often used in combination with <see cref="GetForegroundWindow"/> to determine the process
    /// that owns the currently active window.
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}
