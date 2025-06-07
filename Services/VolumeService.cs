using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;

namespace SerialVolumeControl.Services;

/// <summary>
/// Provides functionality to get and set volume levels for specific applications and the system master volume.
/// </summary>
public static class VolumeService
{
    /// <summary>
    /// Sets the volume level for a specific application's audio session.
    /// </summary>
    /// <param name="appName">The name of the application (process name, case-insensitive) to modify volume for.</param>
    /// <param name="volume">The desired volume level between 0.0 (mute) and 1.0 (max).</param>
    public static void SetAppVolume(string appName, float volume)
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessionManager = device.AudioSessionManager;
            var sessions = sessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                if (session is AudioSessionControl sessionControl)
                {
                    try
                    {
                        int pid = (int)sessionControl.GetProcessID;
                        var process = Process.GetProcessById(pid);
                        if (process != null &&
                            process.ProcessName.Equals(appName, StringComparison.OrdinalIgnoreCase))
                        {
                            sessionControl.SimpleAudioVolume.Volume = volume;
                            Console.WriteLine($"[INFO] Set volume of {appName} to {volume * 100}%");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARN] Could not access process for session {i}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to set volume for app '{appName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves the current volume level of a specific application's audio session.
    /// </summary>
    /// <param name="appName">The name of the application (process name, case-insensitive) to retrieve volume for.</param>
    /// <returns>The volume level between 0.0 and 1.0, or -1 if the application is not found or an error occurs.</returns>
    public static float GetAppVolume(string appName)
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessionManager = device.AudioSessionManager;
            var sessions = sessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                if (session is AudioSessionControl sessionControl)
                {
                    int pid = (int)sessionControl.GetProcessID;
                    var process = Process.GetProcessById(pid);
                    if (process != null &&
                        process.ProcessName.Equals(appName, StringComparison.OrdinalIgnoreCase))
                    {
                        return sessionControl.SimpleAudioVolume.Volume;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get volume for app '{appName}': {ex.Message}");
        }

        return -1;
    }

    /// <summary>
    /// Sets the master (system-wide) volume level.
    /// </summary>
    /// <param name="volume">The desired volume level between 0.0 (mute) and 1.0 (max).</param>
    public static void SetMasterVolume(float volume)
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(volume, 0f, 1f);
            Console.WriteLine($"[INFO] Set master volume to {volume * 100}%");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to set master volume: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves the current master (system-wide) volume level.
    /// </summary>
    /// <returns>The volume level between 0.0 and 1.0, or -1 if an error occurs.</returns>
    public static float GetMasterVolume()
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device.AudioEndpointVolume.MasterVolumeLevelScalar;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get master volume: {ex.Message}");
            return -1;
        }
    }
}
