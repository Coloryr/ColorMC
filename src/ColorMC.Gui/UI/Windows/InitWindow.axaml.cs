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
            BaseBinding.Init();

            if (GuiConfigUtils.Config != null)
            {
                await App.LoadImage(GuiConfigUtils.Config.BackImage,
                    GuiConfigUtils.Config.BackEffect);
            }

            Dispatcher.UIThread.Post(() =>
            {
                InitDone = true;
                if (BaseBinding.ISNewStart)
                {
                    App.ShowHello();
                }
                else
                {
                    ShowMain();
                }
                Close();
            });
        });
    }

    private void ShowMain()
    {
        bool ok;
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 == null || string.IsNullOrWhiteSpace(config.Item2.ServerCustom.UIFile))
        {
            ok = false;
        }
        else
        {
            try
            {
                string file = config.Item2.ServerCustom.UIFile;
                if (File.Exists(file))
                {
                    var obj = JsonConvert.DeserializeObject<UIObj>(File.ReadAllText(file));
                    if (obj == null)
                    {
                        ok = false;
                    }
                    else
                    {
                        App.ShowCustom(obj);
                        ok = true;
                    }
                }
                else
                {
                    ok = false;
                }
            }
            catch (Exception e)
            {
                CoreMain.OnError?.Invoke("自定义UI加载失败", e, true);
                ok = false;
            }
        }

        if (!ok)
        {
            App.ShowMain();
        }
    }
}
