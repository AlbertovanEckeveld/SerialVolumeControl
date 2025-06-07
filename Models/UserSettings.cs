using System.Collections.Generic;

namespace SerialVolumeControl.Models;

public class UserSettings
{
    public string? LastComPort { get; set; }
    public Dictionary<string, float>? AppVolumes { get; set; }
    public List<string?>? SliderAppAssignments { get; set; }
    public bool IsDarkMode { get; set; }
}