using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.X11;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui;
using ColorMC.Gui.UI.Views.Main;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI;

public partial class MainWindow : Window
{
    public static MainWindow Window;

    private List<GamesControl> Groups = new();
    private GamesControl DefaultGroup;
    private GameSettingObj? Obj;
    private LoginObj? SelectUser;

    public MainWindow()
    {
        Window = this;

        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        ItemInfo.SetWindow(this);

        Task.Run(Load);
        Task.Run(Load1);

        string file = GuiConfigUtils.Config?.BackImage;
        if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
        {
            Image_Back.Source = new Bitmap(file);
        }

        Image_Back.Source = new Bitmap("F:\\illust_94899568_20220104_002837.png");

        Opened += MainWindow_Opened;
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        ItemInfo.Expander1.MakeExpanderTran();
    }

    public void ItemSelect(GameSettingObj? obj)
    {
        Obj = obj;
    }

    private void Load1()
    {
        SelectUser = UserBinding.GetLastUser();
        Dispatcher.UIThread.Post(() => { ItemInfo.SetUser(SelectUser); });
    }

    private async void Load()
    {
        var list = OtherBinding.GetGameGroups();

        Groups.Clear(); 
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DefaultGroup = new();
        });
        DefaultGroup.SetWindow(this);
        foreach (var item in list)
        {
            if (item.Key == " ")
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    DefaultGroup.SetItems(item.Value);
                    DefaultGroup.SetName("Ä¬ÈÏ·Ö×é");
                });
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var group = new GamesControl();
                    group.SetItems(item.Value);
                    group.SetName(item.Key);
                    group.SetWindow(this);
                    Groups.Add(group);
                });
            }
        }

        Dispatcher.UIThread.Post(() =>
        {
            GameGroups.Children.Clear();
            foreach (var item in Groups)
            {
                GameGroups.Children.Add(item);
            }
            GameGroups.Children.Add(DefaultGroup);
        });
    }
}
