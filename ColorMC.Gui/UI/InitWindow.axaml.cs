using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core;
using System;
using System.IO;
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
            GuiConfigUtils.Init(AppContext.BaseDirectory);

            string file = GuiConfigUtils.Config?.BackImage;
            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
            {
                App.BackBitmap = new Bitmap(file);
            }

            App.BackBitmap = new Bitmap("F:\\illust_94899568_20220104_002837.png");

            Dispatcher.UIThread.Post(() =>
            {
                App.ShowMain();
                Close();
            });
        });
    }
}
