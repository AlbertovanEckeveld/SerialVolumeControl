using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;

namespace SerialVolumeControl.Services;

public static class VolumeService
{
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
}
