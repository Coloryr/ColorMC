using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.ComponentModel;

namespace ColorMC.Launcher;

public partial class MainWindow : Window
{
    private bool IsClose;
    public MainWindow()
    {
        InitializeComponent();

        FontFamily = Program.Font;

        Opened += MainWindow_Opened;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        IsClose = true;
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        ProgressBar1.Maximum = 4;

        try
        {
            await Program.updater.Download(State);

            Label1.IsVisible = true;

            if (!IsClose)
            {
                Program.Launch();
            }
            App.Exit();
        }
        catch
        {
            Label1.Content = "更新失败";
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
