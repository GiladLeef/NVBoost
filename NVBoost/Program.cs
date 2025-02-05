using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.ReactiveUI;
using nvboost.NVML;

namespace nvboost;

sealed class Program
{
    public static string DefaultDataPath =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.nvboost-gui";

    public static Process? FanCurveProcess = null;

    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);


    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}