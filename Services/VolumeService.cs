using System;
using System.Diagnostics;

using NAudio.CoreAudioApi;


namespace SerialVolumeControl.Services;

public static class VolumeService
{
    // function to set the volume of a specific application
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

    // function to get the volume of a specific application
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

    // Set the master/system volume (0.0 - 1.0)
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

    // Get the master/system volume (0.0 - 1.0)
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
