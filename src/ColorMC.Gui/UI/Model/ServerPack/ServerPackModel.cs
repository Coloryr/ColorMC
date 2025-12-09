using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

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

    private readonly string _useName;

    public ServerPackModel(WindowModel model, ServerPackObj obj) : base(model)
    {
        Obj = obj;

        _useName = ToString() ?? "ServerPackModel";

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = LangUtils.Get("ServerPackWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = LangUtils.Get("Type.FileType.Mod"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = SelectAllMod,
                        Name = LangUtils.Get("Button.SelectAll")
                    },
                    new SubMenuItemModel()
                    {
                        Func = UnSelectAllMod,
                        Name = LangUtils.Get("ServerPackWindow.Tab2.Text3")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item3.svg",
                Text = LangUtils.Get("Type.FileType.Resourcepack"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = SelectAllResource,
                        Name = LangUtils.Get("Button.SelectAll")
                    },
                    new SubMenuItemModel()
                    {
                        Func = UnSelectAllResource,
                        Name = LangUtils.Get("ServerPackWindow.Tab2.Text3")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item4.svg",
                Text = LangUtils.Get("Text.Config"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = Gen,
                        Name = LangUtils.Get("ServerPackWindow.Tab1.Text10")
                    }
                ]
            },
        ]);

        Window.SetChoiseContent(_useName, LangUtils.Get("ServerPackWindow.Tab1.Text10"));
        Window.SetChoiseCall(_useName, Gen);
    }

    /// <summary>
    /// 开始生成
    /// </summary>
    public async void Gen()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var local = await PathBinding.SelectPathAsync(top, PathType.ServerPackPath);
        if (local == null)
        {
            return;
        }

        Obj.Text = Text;

        var dialog = Window.ShowProgress(LangUtils.Get("ServerPackWindow.Tab1.Text12"));
        var res = await Obj.GenServerPackAsync(local);
        Window.CloseDialog(dialog);
        if (res)
        {
            Window.Notify(LangUtils.Get("ServerPackWindow.Tab1.Text13"));
        }
        else
        {
            Window.Show(LangUtils.Get("ServerPackWindow.Tab1.Text11"));
        }
    }

    public override void Close()
    {
        Window.RemoveChoiseData(_useName);
        ModList.Clear();
        ResourceList.Clear();
        NameList.Clear();
    }
}
