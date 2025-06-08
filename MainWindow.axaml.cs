using System.Collections.Generic;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;

using SerialVolumeControl.Services;

namespace SerialVolumeControl
{
    /// <summary>
    /// Represents the main window of the SerialVolumeControl application.
    /// Manages the user interface, serial communication, settings, and UI interactions.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The list of ComboBoxes for application selection per slider.
        /// </summary>
        private readonly List<ComboBox> _appComboBoxes = new();

        /// <summary>
        /// The list of sliders that control volume levels.
        /// </summary>
        private readonly List<Slider> _volumeSliders = new();

        /// <summary>
        /// Maps each slider index to its assigned application.
        /// </summary>
        private readonly Dictionary<int, string?> _sliderAppAssignments = new();

        /// <summary>
        /// Serial reader instance for handling communication with the hardware.
        /// </summary>
        private readonly SerialReader _reader = new();

        /// <summary>
        /// Stores saved volume levels for each application.
        /// </summary>
        private Dictionary<string, float> _savedAppVolumes = new();

        /// <summary>
        /// The last used COM port.
        /// </summary>
        private string? _lastComPort;

        /// <summary>
        /// Stores assignments for each slider from previous sessions.
        /// </summary>
        private List<string?> _sliderAppAssignmentsList = new() { null, null, null, null, null };

        /// <summary>
        /// Reference to the UI toggle button for switching between light and dark themes.
        /// </summary>
        private ToggleButton? _themeToggle;

        /// <summary>
        /// Indicates whether the current theme is dark mode.
        /// </summary>
        private bool _isDarkMode = false;

        /// <summary>
        /// The system tray icon for the application.
        /// </summary>
        private TrayIcon? _trayIcon;

        /// <summary>
        /// The context menu associated with the system tray icon.
        /// </summary>
        private NativeMenu? _trayMenu;

        /// <summary>
        /// Indicates whether an exit request has been made (used to prevent redundant actions on shutdown).
        /// </summary>
        private bool _isExitRequested = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up the UI, loads saved settings, initializes controls and hardware communication.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            LoadSettings();

            _themeToggle = this.FindControl<ToggleButton>("ThemeToggleButton");

            InitializeComboBoxesAndSliders();
            InitializePortControls();
            InitializeDropdowns();
            InitializeSliderAssignments();
            InitializeSerialReader();

            SetupThemeToggle();
            SetTheme(_isDarkMode);
            SetupTrayIcon();
        }
    }
}
