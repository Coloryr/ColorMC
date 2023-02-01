using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class InitWindow : Window
{
    private bool InitDone = false;
    public InitWindow()
    {
        InitializeComponent();

        FontFamily = Program.Font;

        Icon = App.Icon;

        Rectangle1.MakeResizeDrag(this);

        Opened += MainWindow_Opened;
        Closed += InitWindow_Closed;
    }

    private void InitWindow_Closed(object? sender, EventArgs e)
    {
        if (!InitDone)
        {
            App.Close();
        }
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        Task.Run(async () =>
        {
            await BaseBinding.Init();

            Dispatcher.UIThread.Post(() =>
            {
                InitDone = true;
                if (BaseBinding.ISNewStart)
                {
                    App.ShowHello();
                }
                else
                {
                    App.ShowCustom();
                }
                Close();
            });
        });
    }
}
