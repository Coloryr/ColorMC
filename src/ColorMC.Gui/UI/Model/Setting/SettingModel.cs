using System.Timers;
using Avalonia.Threading;
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
    /// 是否为手机模式
    /// </summary>
    public bool Phone { get; init; }
    /// <summary>
    /// 是否启用手柄
    /// </summary>
    public bool IsInputEnable { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _enableWindowMode = true;

    /// <summary>
    /// 更新定制器
    /// </summary>
    private readonly Timer _timer;

    public SettingModel(BaseModel model) : base(model)
    {
#if Phone
        Phone = true;
        _enableWindowMode = false;
#endif

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
                Text = App.Lang("SettingWindow.Tabs.Text2")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item2.svg",
                Text = App.Lang("SettingWindow.Tabs.Text3")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item3.svg",
                Text = App.Lang("SettingWindow.Tabs.Text4")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item4.svg",
                Text = App.Lang("SettingWindow.Tabs.Text5"),
                SubMenu =
                [
                     new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.RefashList"),
                        Func = LoadJava
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab5.Text4"),
                        Func = ShowAddJava
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab5.Text7"),
                        Func = FindJava,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab5.Text12"),
                        Func = FindJavaDir,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab5.Text5"),
                        Func = DeleteJava
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab5.Text8"),
                        Func = AddJavaZip
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item5.svg",
                Text = App.Lang("SettingWindow.Tabs.Text6"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab6.Text43"),
                        Func = ShowBuildPack
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item6.svg",
                Text = App.Lang("SettingWindow.Tabs.Text1"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab1.Text1"),
                        Func = Reset
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab1.Text18"),
                        Func = DumpUser,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab1.Text2"),
                        Func = ClearUser
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab1.Text19"),
                        Func = ClearWindow,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab1.Text3"),
                        Func = Open,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab5.Text6"),
                        Func = OpenJavaPath,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab3.Text13"),
                        Func = OpenDownloadPath,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab3.Text18"),
                        Func = OpenPicPath,
                        Hide = Phone
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item8.svg",
                Text = App.Lang("SettingWindow.Tabs.Text8"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab8.Info8"),
                        Func = ReloadInput,
                        Hide = Phone
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item7.svg",
                Text = App.Lang("SettingWindow.Tabs.Text7")
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
            Memory = string.Format(App.Lang("SettingWindow.Tab4.Text29"), HookUtils.GetMemorySize(), HookUtils.GetFreeSize());
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
