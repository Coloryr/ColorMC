using Avalonia.Controls;
using Avalonia.Threading;
using System;

namespace ColorMC.Launcher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        FontFamily = Program.Font;

        Opened += MainWindow_Opened;
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        ProgressBar1.Maximum = 4;

        try
        {
            await Program.updater.Download(State);

            Label1.IsVisible = true;
            Program.Launch();
            App.Exit();
        }
        catch
        {
            Label1.Content = "¸üÐÂÊ§°Ü";
            Label1.IsVisible = true;
        }
    }

    private void State(int state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ProgressBar1.Value = state;
        });
    }
}
