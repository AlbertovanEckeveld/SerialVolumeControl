using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SerialVolumeControl;

/// <summary>
/// Represents the application entry point for the SerialVolumeControl Avalonia application.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the application and loads the XAML markup.
    /// Called during the application startup.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when the Avalonia framework has completed initialization.
    /// Sets the main window for desktop-style application lifetimes.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Set the main window of the application.
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
