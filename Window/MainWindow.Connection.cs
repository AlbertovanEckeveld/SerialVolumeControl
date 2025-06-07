using System;
using Avalonia.Controls;

namespace SerialVolumeControl
{
    /// <summary>
    /// Partial class containing serial connection logic for the main application window.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Establishes a connection to the selected serial port.
        /// </summary>
        /// <remarks>
        /// Retrieves UI elements by name, connects to the selected COM port via the internal serial reader,
        /// updates UI controls accordingly, and saves the current settings.
        /// </remarks>
        private void Connect()
        {
            try
            {
                var portComboBox = this.FindControl<ComboBox>("PortComboBox");
                var connectButton = this.FindControl<Button>("ConnectButton");
                var disconnectButton = this.FindControl<Button>("DisconnectButton");

                if (portComboBox?.SelectedItem is string portName)
                {
                    _reader.Connect(portName);

                    if (connectButton != null) connectButton.IsEnabled = false;
                    if (portComboBox != null) portComboBox.IsEnabled = false;
                    if (disconnectButton != null) disconnectButton.IsEnabled = true;

                    _lastComPort = portName;
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to serial port: {ex}");
            }
        }

        /// <summary>
        /// Terminates the existing serial port connection.
        /// </summary>
        /// <remarks>
        /// Calls the disconnect method on the serial reader, re-enables relevant UI controls,
        /// and persists the settings.
        /// </remarks>
        private void Disconnect()
        {
            try
            {
                var portComboBox = this.FindControl<ComboBox>("PortComboBox");
                var connectButton = this.FindControl<Button>("ConnectButton");
                var disconnectButton = this.FindControl<Button>("DisconnectButton");

                _reader.Disconnect();

                if (connectButton != null) connectButton.IsEnabled = true;
                if (portComboBox != null) portComboBox.IsEnabled = true;
                if (disconnectButton != null) disconnectButton.IsEnabled = false;

                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from serial port: {ex}");
            }
        }
    }
}
