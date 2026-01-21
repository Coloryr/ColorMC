using System.Timers;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.Hook;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel : MenuModel
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    private readonly GameSettingObj _obj;

    /// <summary>
    /// 更新内存用
    /// </summary>
    private readonly Timer _timer;

    public GameEditModel(WindowModel model, GameSettingObj obj) : base(model)
    {
        _obj = obj;

        _timer = new Timer(1000);
        _timer.Elapsed += Timer_Elapsed;
        _timer.AutoReset = true;

        //加载设置
        _setting = GameManager.ReadConfig(_obj);

        _titleText = string.Format(LangUtils.Get("GameEditWindow.Tab2.Text13"), _obj.Name);

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item1.svg",
                Text = LangUtils.Get("GameEditWindow.Tabs.Text1"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab1.Text11"),
                        Func = ExportGame
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab1.Text7"),
                        Func = OpenGameLog
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab1.Text6"),
                        Func = OpenConfigEdit
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.OpFile"),
                        Func = OpPath
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab1.Text5"),
                        Func = OpenServerPack
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab1.Text14"),
                        Func = GenGameInfo
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.Rename"),
                        Func = Rename
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab1.Text16"),
                        Func = Delete
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item2.svg",
                Text = LangUtils.Get("MainWindow.Text64"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab2.Text28"),
                        Func = DeleteConfig
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item3.svg",
                Text = LangUtils.Get("Type.FileType.Mod"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.RefashList"),
                        Func = LoadMods
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab4.Text6"),
                        Func = ImportMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.NetDownload"),
                        Func = AddMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.CheckUpdate"),
                        Func = CheckMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab4.Text7"),
                        Func = StartAutoSetMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab4.Text4"),
                        Func = StartSetMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab4.Text5"),
                        Func = DependTestMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.OpFile"),
                        Func = OpenMod
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item4.svg",
                Text = LangUtils.Get("GameEditWindow.Tabs.Text5"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.Refash"),
                        Func = LoadWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab5.Text4"),
                        Func = ImportWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.NetDownload"),
                        Func = AddWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab5.Text3"),
                        Func = EditWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab5.Text2"),
                        Func = BackupWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab5.Text1"),
                        Func = OpenBackupWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.OpFile"),
                        Func = OpenWorld
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item5.svg",
                Text = LangUtils.Get("Type.FileType.Resourcepack"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.Refash"),
                        Func = LoadResource
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab8.Text1"),
                        Func = ImportResource
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.NetDownload"),
                        Func = AddResource
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.OpFile"),
                        Func = OpenResource
                    },
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item6.svg",
                Text = LangUtils.Get("GameEditWindow.Tabs.Text7"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.RefashList"),
                        Func = LoadScreenshot
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab9.Text3"),
                        Func = ClearScreenshot
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.OpFile"),
                        Func = OpenScreenshot
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item7.svg",
                Text = LangUtils.Get("GameEditWindow.Tabs.Text10"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.RefashList"),
                        Func = LoadServer
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab10.Text3"),
                        Func = AddServer
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item8.svg",
                Text = LangUtils.Get("Type.FileType.Shaderpack"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.RefashList"),
                        Func = LoadShaderpack
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab11.Text1"),
                        Func = ImportShaderpack
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.NetDownload"),
                        Func = AddShaderpack
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.OpFile"),
                        Func = OpenShaderpack
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item9.svg",
                Text = LangUtils.Get("Text.Schematic"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Text.RefashList"),
                        Func = LoadSchematic
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("GameEditWindow.Tab12.Text1"),
                        Func = AddSchematic
                    },
                    new SubMenuItemModel()
                    {
                        Name = LangUtils.Get("Button.OpFile"),
                        Func = OpenSchematic
                    }
                ]
            },
        ]);
    }

    /// <summary>
    /// 开始读取内存大小
    /// </summary>
    public void Load()
    {
        _timer.Start();
    }

    /// <summary>
    /// 更新内存占用
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Memory = string.Format(LangUtils.Get("SettingWindow.Tab4.Text29"),
                HookUtils.GetMemorySize(), HookUtils.GetFreeSize());
        });
    }

    public override void Close()
    {
        _timer.Stop();
        _timer.Dispose();
        _configLoad = true;
        _gameLoad = true;
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        GroupList.Clear();
        JvmList.Clear();
        _root.Clear();
        _modItems.Clear();
        _displayModList.Clear();
        foreach (var item in WorldList)
        {
            item.Close();
        }
        WorldList.Clear();
        _selectWorld = null;
        foreach (var item in ResourcePackList)
        {
            item.Close();
        }
        ResourcePackList.Clear();
        _lastResource = null;
        foreach (var item in ScreenshotList)
        {
            item.Close();
        }
        ScreenshotList.Clear();
        _lastScreenshot = null!;
        ServerList.Clear();
        ShaderpackList.Clear();
        SchematicList.Clear();
    }
}
