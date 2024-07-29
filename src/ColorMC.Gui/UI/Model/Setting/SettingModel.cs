using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel : MenuModel
{
    public bool Phone { get; } = false;
    public bool IsInputEnable { get; }

    private readonly string _name;

    public SettingModel(BaseModel model) : base(model)
    {
        _name = ToString() ?? "SettingModel";

        if (SystemInfo.Os == OsType.Android)
        {
            Phone = true;
            _enableWindowMode = false;
        }
        else if (SystemInfo.Os is OsType.Windows)
        {
            IsInputEnable = true;
        }

        if (!BaseBinding.SdlInit)
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
                        Func = FindJava
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
                Text = App.Lang("SettingWindow.Tabs.Text6")
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
                        Func = DumpUser
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab1.Text2"),
                        Func = ClearUser
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab1.Text3"),
                        Func = Open
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab5.Text6"),
                        Func = OpenJavaPath
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab3.Text13"),
                        Func = OpenDownloadPath
                    },
                    new SubMenuItemModel()
                    {
                        Name = App.Lang("SettingWindow.Tab3.Text18"),
                        Func = OpenPicPath
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item8.svg",
            Text = App.Lang("SettingWindow.Tabs.Text8")
            },
            new()
            {
                Icon = "/Resource/Icon/Setting/item7.svg",
                Text = App.Lang("SettingWindow.Tabs.Text7")
            }
        ]);
    }

    public void RemoveChoise()
    {
        Model.RemoveChoiseData(_name);
    }

    public override void Close()
    {
        FontList.Clear();
        JavaList.Clear();
        _uuids.Clear();
        InputClose();
        StopRead();
    }
}
