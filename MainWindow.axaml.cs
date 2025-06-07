using System.Collections.Generic;

using Avalonia.Controls;
using SerialVolumeControl.Services;
using Avalonia.Controls.Primitives;

namespace SerialVolumeControl
{
    public partial class MainWindow : Window
    {
        private readonly List<ComboBox> _appComboBoxes = new();
        private readonly List<Slider> _volumeSliders = new();
        private readonly Dictionary<int, string?> _sliderAppAssignments = new();
        private readonly SerialReader _reader = new();

        private Dictionary<string, float> _savedAppVolumes = new();
        private string? _lastComPort;
        private List<string?> _sliderAppAssignmentsList = new() { null, null, null, null, null };
        private ToggleButton? _themeToggle;
        private bool _isDarkMode = false;

        private TrayIcon? _trayIcon;
        private NativeMenu? _trayMenu;
        private bool _isExitRequested = false;

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