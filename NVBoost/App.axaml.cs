using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using nvboost.ViewModels;
using nvboost.Views;
using nvboost.Models;

namespace nvboost;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line, you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            // No need for null check on desktop, as Avalonia guarantees it will not be null
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        // Make sure that WindowsManager.AllWindows is not null before accessing it
        var mainWindow = WindowsManager.AllWindows?.FirstOrDefault(x => x.Name == "MainOcWindow");

        if (mainWindow != null)
        {
            mainWindow.Show();
        }
    }

    private void NativeMenuItem_OnClick(object? sender, EventArgs e)
    {
        // It's safe to exit here, no need to check for null
        Environment.Exit(0);
    }
}
