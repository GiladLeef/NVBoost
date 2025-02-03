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
            
            
            BindingPlugins.DataValidators.RemoveAt(0);

            
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        
        var mainWindow = WindowsManager.AllWindows?.FirstOrDefault(x => x.Name == "MainOcWindow");

        if (mainWindow != null)
        {
            mainWindow.Show();
        }
    }

    private void NativeMenuItem_OnClick(object? sender, EventArgs e)
    {
        
        Environment.Exit(0);
    }
}
