using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using System;
using Avalonia.Platform;

namespace ColorMC.Launcher;

public partial class Window1 : Window
{
    public Window1()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        using var asset1 = AssetLoader.Open(new Uri("resm:ColorMC.Gui.icon.ico?assembly=ColorMC.Gui"));
        Icon = new(asset1!);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var startInfo = new ProcessStartInfo("ColorMC.Launcher.exe")
        {
            Verb = "runas",
            UseShellExecute = true
        };
        Process.Start(startInfo);
        Close();
    }
}
