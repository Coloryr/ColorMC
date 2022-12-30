using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI;

public partial class InitWindow : Window
{
    public InitWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        Opened += MainWindow_Opened;
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        Task.Run(() =>
        {
            CoreMain.Init(AppContext.BaseDirectory);

            Dispatcher.UIThread.Post(() =>
            {
                App.ShowMain();
                Close();
            });
        });
    }
}
