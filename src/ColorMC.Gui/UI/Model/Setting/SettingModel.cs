using System.Timers;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置窗口
/// </summary>
public partial class SettingModel : MenuModel
{
    /// <summary>
    /// 是否启用手柄
    /// </summary>
    public bool IsInputEnable { get; private set; }

    public bool IsWindows { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _enableWindowMode = true;

    /// <summary>
    /// 更新定制器
    /// </summary>
    private readonly Timer _timer;

    public SettingModel(WindowModel model) : base(model)
    {
        IsWindows = SystemInfo.Os == OsType.Windows;

        //更新定制器用于内存
        _timer = new Timer(1000);
        _timer.Elapsed += Timer_Elapsed;
        _timer.AutoReset = true;

        if (!SdlUtils.SdlInit)
        {
            InputInit = false;
        }
        else
        {
            InputInit = true;
            StartRead();
            ReloadInput();
        }

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/Setting/item1.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text2")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item2.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text3")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item3.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text4")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item4.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text5"),
                SubMenu =
                [
                     new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.RefashList"),
                        Func = LoadJava
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab5.Text4"),
                        Func = ShowAddJava
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab5.Text7"),
                        Func = FindJava
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab5.Text12"),
                        Func = FindJavaDir
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab5.Text5"),
                        Func = DeleteJava
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab5.Text8"),
                        Func = AddJavaZip
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item5.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text6"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab6.Text43"),
                        Func = ShowBuildPack
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item6.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text1"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab1.Text1"),
                        Func = Reset
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab1.Text18"),
                        Func = DumpUser
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab1.Text2"),
                        Func = ClearUser
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab1.Text19"),
                        Func = ClearWindow
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab1.Text3"),
                        Func = Open
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab5.Text6"),
                        Func = OpenJavaPath
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab3.Text13"),
                        Func = OpenDownloadPath
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab3.Text18"),
                        Func = OpenPicPath
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item8.svg",
                Text = LangUtils.Get("MainWindow.Text56"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("SettingWindow.Tab8.Text42"),
                        Func = ReloadInput
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item8.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text9")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item7.svg",
                Text = LangUtils.Get("SettingWindow.Tabs.Text7")
            }
        ]);
    }

    /// <summary>
    /// 加载基础设置
    /// </summary>
    public void Load()
    {
        _timer.Start();
        LoadUISetting();
    }

    /// <summary>
    /// 更新系统内存
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Memory = string.Format(LangUtils.Get("SettingWindow.Tab4.Text29"), HookUtils.GetMemorySize(), HookUtils.GetFreeSize());
        });
    }

    public override void Close()
    {
        _timer.Stop();
        _timer.Dispose();
        FontList.Clear();
        JavaList.Clear();
        _uuids.Clear();
        InputClose();
        StopRead();
    }
}
