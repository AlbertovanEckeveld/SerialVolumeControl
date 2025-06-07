using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

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

    private static readonly string SettingsFile = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "SerialVolumeControl", "app_volumes.json");

    // Call this to save all app volumes (e.g., when user changes a slider)
    public static void SaveAppVolumes(Dictionary<string, float> appVolumes)
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            var json = JsonSerializer.Serialize(appVolumes);
            File.WriteAllText(SettingsFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to save app volumes: {ex.Message}");
        }
    }

    // Call this on startup to restore all app volumes
    public static Dictionary<string, float> LoadAppVolumes()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<Dictionary<string, float>>(json)
                       ?? new Dictionary<string, float>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to load app volumes: {ex.Message}");
        }
        return new Dictionary<string, float>();
    }
}
