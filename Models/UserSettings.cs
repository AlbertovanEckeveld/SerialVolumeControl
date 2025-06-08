using System.Collections.Generic;

namespace SerialVolumeControl.Models;

/// <summary>
/// Represents user-configurable settings for the SerialVolumeControl application.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Gets or sets the last used COM port identifier (e.g., "COM3").
    /// </summary>
    public string? LastComPort { get; set; }

    /// <summary>
    /// Gets or sets a dictionary mapping application names to their last known volume levels.
    /// The key is the application name or identifier, and the value is the volume level (0.0 to 1.0).
    /// </summary>
    public Dictionary<string, float>? AppVolumes { get; set; }

    /// <summary>
    /// Gets or sets a list of slider-to-application assignments.
    /// Each item corresponds to a slider index, and its value is the assigned application name or a special option.
    /// </summary>
    public List<string?>? SliderAppAssignments { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether dark mode is enabled in the UI.
    /// </summary>
    public bool IsDarkMode { get; set; }
}