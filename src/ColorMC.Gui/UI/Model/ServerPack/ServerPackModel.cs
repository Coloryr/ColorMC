using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.ServerPack;

/// <summary>
/// 服务器包生成
/// </summary>
public partial class ServerPackModel : MenuModel
{
    /// <summary>
    /// 服务器包
    /// </summary>
    public ServerPackObj Obj { get; }

    public ServerPackModel(BaseModel model, ServerPackObj obj) : base(model)
    {
        Obj = obj;

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = App.Lang("ServerPackWindow.Tabs.Text1"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = Gen,
                        Name = App.Lang("ServerPackWindow.Tab1.Text10")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = App.Lang("ServerPackWindow.Tabs.Text2"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = SelectAllMod,
                        Name = App.Lang("Button.SelectAll")
                    },
                    new SubMenuItemModel()
                    {
                        Func = UnSelectAllMod,
                        Name = App.Lang("ServerPackWindow.Tab2.Text3")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item3.svg",
                Text = App.Lang("ServerPackWindow.Tabs.Text3"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = SelectAllResource,
                        Name = App.Lang("Button.SelectAll")
                    },
                    new SubMenuItemModel()
                    {
                        Func = UnSelectAllResource,
                        Name = App.Lang("ServerPackWindow.Tab2.Text3")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item4.svg",
                Text = App.Lang("ServerPackWindow.Tabs.Text4"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = Gen,
                        Name = App.Lang("ServerPackWindow.Tab1.Text10")
                    }
                ]
            },
        ]);
    }

    /// <summary>
    /// 开始生成
    /// </summary>
    public async void Gen()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var local = await PathBinding.SelectPath(top, PathType.ServerPackPath);
        if (local == null)
        {
            return;
        }

        Obj.Text = Text;

        Model.Progress(App.Lang("ServerPackWindow.Tab1.Info1"));
        var res = await GameBinding.GenServerPack(Obj, local, Model.ShowAsync);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.Lang("ServerPackWindow.Tab1.Info2"));
        }
        else
        {
            Model.Show(App.Lang("ServerPackWindow.Tab1.Error3"));
        }
    }

    public override void Close()
    {
        ModList.Clear();
        ResourceList.Clear();
        NameList.Clear();
    }
}
