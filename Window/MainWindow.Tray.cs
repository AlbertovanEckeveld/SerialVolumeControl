using System;
using Avalonia.Controls;

namespace SerialVolumeControl
{
    /// <summary>
    /// Partial class for the main application window, responsible for handling tray icon behavior,
    /// window visibility toggling, and application exit logic.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Initializes the system tray icon with a menu that includes
        /// "Show/Hide" and "Exit" actions. Handles click events for toggling window visibility.
        /// </summary>
        /// <remarks>
        /// - Sets the tray icon with a tooltip and icon image.
        /// - Handles menu item clicks to control application behavior.
        /// - Errors during setup are caught and logged to the console.
        /// </remarks>
        private void SetupTrayIcon()
        {
            try
            {
                _trayMenu = new NativeMenu();

                var showHideItem = new NativeMenuItem("Show/Hide");
                showHideItem.Click += (_, _) => ToggleWindowVisibility();
                _trayMenu.Add(showHideItem);

                _trayMenu.Add(new NativeMenuItemSeparator());

                var exitItem = new NativeMenuItem("Exit");
                exitItem.Click += (_, _) => ExitApp();
                _trayMenu.Add(exitItem);

                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon("Assets/trayicon.ico"),
                    ToolTipText = "SerialVolumeControl",
                    Menu = _trayMenu
                };
                _trayIcon.Clicked += (_, _) => ToggleWindowVisibility();
                _trayIcon.IsVisible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tray icon setup failed: " + ex);
            }
        }

        /// <summary>
        /// Toggles the visibility of the main application window.
        /// </summary>
        /// <remarks>
        /// - If the window is hidden or minimized, it will be shown and brought to the foreground.
        /// - If the window is currently visible, it will be hidden.
        /// </remarks>
        private void ToggleWindowVisibility()
        {
            if (this.WindowState == WindowState.Minimized || !this.IsVisible)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
            else
            {
                this.Hide();
            }
        }

        /// <summary>
        /// Cleans up and exits the application when the "Exit" tray menu item is clicked.
        /// </summary>
        /// <remarks>
        /// - Sets a flag to allow window closing.
        /// - Disposes the tray icon.
        /// - Closes the window/application.
        /// </remarks>
        private void ExitApp()
        {
            _isExitRequested = true;
            _trayIcon?.Dispose();
            Close();
        }

        /// <summary>
        /// Overrides the window closing event to implement "minimize to tray" behavior.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <remarks>
        /// - If the exit flag is not set, the window is hidden instead of being closed.
        /// - If the exit flag is set, the application exits normally.
        /// </remarks>
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (!_isExitRequested)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                base.OnClosing(e);
            }
        }
    }
}