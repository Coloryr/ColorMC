using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Gui.UIBinding;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

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
        Task.Run(async () =>
        {
            BaseBinding.Init();

            if (GuiConfigUtils.Config != null)
            {
                await App.LoadImage(GuiConfigUtils.Config.BackImage,
                    GuiConfigUtils.Config.BackEffect);
            }

            Dispatcher.UIThread.Post(() =>
            {
                App.ShowMain();
                Close();
            });
        });
    }
}
