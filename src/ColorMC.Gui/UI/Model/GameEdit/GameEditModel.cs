using System.Collections.Generic;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : MenuModel
{
    [ObservableProperty]
    private bool _displayFilter = true;

    private readonly GameSettingObj _obj;

    public bool Phone { get; } = false;

    private readonly string _useName;

    public GameEditModel(BaseModel model, GameSettingObj obj) : base(model)
    {
        _useName = ToString() + ":" + obj.UUID;

        _obj = obj;
        if (SystemInfo.Os == OsType.Android)
        {
            Phone = true;
        }

        _titleText = string.Format(App.Lang("GameEditWindow.Tab2.Text13"), _obj.Name);

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item1.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text1"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab1.Text11"),
                        Func = ExportGame,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab1.Text7"),
                        Func = OpenGameLog
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab1.Text6"),
                        Func = OpenConfigEdit
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.OpFile"),
                        Func = OpPath,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab1.Text5"),
                        Func = OpenServerPack,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab1.Text14"),
                        Func = GenGameInfo
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab1.Text16"),
                        Func = Delete
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item2.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text2"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab2.Text28"),
                        Func = DeleteConfig
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item3.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text4"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab4.Text8"),
                        Func = ImportMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.NetDownload"),
                        Func = AddMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab4.Text3"),
                        Func = CheckMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab4.Text7"),
                        Func = StartAutoSetMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab4.Text4"),
                        Func = StartSetMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab4.Text5"),
                        Func = DependTestMod
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.OpFile"),
                        Func = OpenMod,
                        Hide = Phone
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item4.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text5"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab5.Text4"),
                        Func = ImportWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.NetDownload"),
                        Func = AddWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab5.Text3"),
                        Func = EditWorld,
                        Hide = Phone
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab5.Text2"),
                        Func = BackupWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab5.Text1"),
                        Func = OpenBackupWorld
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.OpFile"),
                        Func = OpenWorld,
                        Hide = Phone
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item5.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text6"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab8.Text1"),
                        Func = ImportResource
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.NetDownload"),
                        Func = AddResource
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.OpFile"),
                        Func = OpenResource,
                        Hide = Phone
                    },
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item6.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text7"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.RefashList"),
                        Func = LoadScreenshot
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Flyouts4.Text1"),
                        Func = ClearScreenshot
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.OpFile"),
                        Func = OpenScreenshot,
                        Hide = Phone
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item7.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text10"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.RefashList"),
                        Func = LoadServer
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab10.Text3"),
                        Func = AddServer
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item8.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text11"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.RefashList"),
                        Func = LoadShaderpack
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab11.Text1"),
                        Func = ImportShaderpack
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.NetDownload"),
                        Func = AddShaderpack
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.OpFile"),
                        Func = OpenShaderpack
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameEdit/item9.svg",
                Text = App.Lang("GameEditWindow.Tabs.Text12"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.RefashList"),
                        Func = LoadSchematic
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("GameEditWindow.Tab12.Text1"),
                        Func = AddSchematic
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("Button.OpFile"),
                        Func = OpenSchematic
                    }
                ]
            },
        ]);
    }

    public void ShowFilter()
    {
        DisplayFilter = !DisplayFilter;
    }

    public void SetChoise()
    {
        Model.SetChoiseContent(_useName, App.Lang("Button.Filter"));
        Model.SetChoiseCall(_useName, ShowFilter);
    }

    public void RemoveChoise()
    {
        Model.RemoveChoiseData(_useName);
    }

    public void OpenLoad()
    {
        GameLoad();
        ConfigLoad();
    }
    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info4"));
            Model.ProgressUpdate(-1);
        }
        else if (state == CoreRunState.End)
        {
            Group = "";
        }
    }

    protected override void Close()
    {
        _configLoad = true;
        _gameLoad = true;
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        GroupList.Clear();
        JvmList.Clear();
        ModList.Clear();
        _modItems.Clear();
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
