using Avalonia;
using Avalonia.Styling;

namespace SerialVolumeControl
{
    /// <summary>
    /// Represents the main window logic for theme handling in the application.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Applies the selected theme (light or dark) to the application and saves the user's preference.
        /// </summary>
        /// <param name="isDark">Indicates whether the dark theme should be applied. If <c>true</c>, the dark theme is set; otherwise, the light theme is used.</param>
        /// <remarks>
        /// This method updates the <see cref="Application.RequestedThemeVariant"/> and persists the change using <c>SaveSettings()</c>.
        /// </remarks>
        private void SetTheme(bool isDark)
        {
            _isDarkMode = isDark;
            Application.Current!.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
            SaveSettings();
        }

        /// <summary>
        /// Configures the toggle control used to switch between dark and light themes.
        /// </summary>
        /// <remarks>
        /// This method initializes the toggle state based on the current theme preference
        /// and wires up an event handler to respond to user interaction.
        /// </remarks>
        private void SetupThemeToggle()
        {
            if (_themeToggle != null)
            {
                _themeToggle.IsChecked = _isDarkMode;
                _themeToggle.IsCheckedChanged += (_, _) =>
                {
                    SetTheme(_themeToggle.IsChecked == true);
                };
            }
        }
    }
}
