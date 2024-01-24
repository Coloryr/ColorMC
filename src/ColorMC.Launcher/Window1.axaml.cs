using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics;
using System.IO;

namespace ColorMC.Launcher;

public partial class Window1 : Window
{
    public Window1()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        using var asset1 = AssetLoader.Open(new Uri("resm:ColorMC.Gui.icon.ico?assembly=ColorMC.Gui"));
        Icon = new(asset1!);
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var res = await StorageProvider.OpenFolderPickerAsync(new()
        {
            Title = "ÔËÐÐÄ¿Â¼(Run Dir)",
            AllowMultiple = false,
            SuggestedStartLocation = await StorageProvider
                .TryGetFolderFromPathAsync(AppDomain.CurrentDomain.BaseDirectory)
        });
        if (res == null || res.Count == 0)
        {
            return;
        }
        var item = res[0].TryGetLocalPath();
        if (item == null)
        {
            return;
        }

        var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/ColorMC/run";
        File.WriteAllText(path, item);

        var startInfo = new ProcessStartInfo("ColorMC.Launcher.exe");
        Process.Start(startInfo);
        Close();
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
