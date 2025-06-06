using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Diagnostics;
using System.Linq; // Added for LINQ methods like FirstOrDefault
using System.Runtime.InteropServices;

namespace SerialVolumeControl.Services
{
    public static class VolumeService
    {
        public static void SetVolume(float volume) // 0.0 - 1.0
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetVolumeWindows(volume);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SetVolumeLinux(volume);
            }
        }

        /// <summary>
        /// Sets the volume for the first process matching the given app name.
        /// </summary>
        /// <param name="appName">The process name (without .exe)</param>
        /// <param name="volume">Volume between 0.0 and 1.0</param>
        public static void SetAppVolume(string appName, float volume)
        {
            var controller = new CoreAudioController();
            var sessions = controller.DefaultPlaybackDevice.SessionController.ActiveSessions(); // Updated method call

            // Find the session for the given app name
            var session = sessions.FirstOrDefault(s =>
                s.ProcessId != 0 && // Updated to use ProcessId instead of Process
                s.DisplayName.Equals(appName, StringComparison.OrdinalIgnoreCase)); // Updated to use DisplayName

            if (session != null)
            {
                session.Volume = volume * 100; // Session.Volume expects 0-100
            }
        }

        private static void SetVolumeWindows(float volume)
        {
            // Windows: NAudio
            var device = new NAudio.CoreAudioApi.MMDeviceEnumerator()
                .GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }

        private static void SetVolumeLinux(float volume)
        {
            // Linux: pactl set-sink-volume
            int percent = (int)(volume * 100);
            var psi = new ProcessStartInfo
            {
                FileName = "pactl",
                Arguments = $"set-sink-volume @DEFAULT_SINK@ {percent}%",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
    }
}
