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

    public ServerPackModel(BaseModel model, ServerPackObj obj) : base(model)
    {
        Obj = obj;

        _useName = ToString() ?? "ServerPackModel";

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = LanguageUtils.Get("ServerPackWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = LanguageUtils.Get("Type.FileType.Mod"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = SelectAllMod,
                        Name = LanguageUtils.Get("Button.SelectAll")
                    },
                    new SubMenuItemModel()
                    {
                        Func = UnSelectAllMod,
                        Name = LanguageUtils.Get("ServerPackWindow.Tab2.Text3")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item3.svg",
                Text = LanguageUtils.Get("Type.FileType.Resourcepack"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = SelectAllResource,
                        Name = LanguageUtils.Get("Button.SelectAll")
                    },
                    new SubMenuItemModel()
                    {
                        Func = UnSelectAllResource,
                        Name = LanguageUtils.Get("ServerPackWindow.Tab2.Text3")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item4.svg",
                Text = LanguageUtils.Get("Text.Config"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = Gen,
                        Name = LanguageUtils.Get("ServerPackWindow.Tab1.Text10")
                    }
                ]
            },
        ]);

        Model.SetChoiseContent(_useName, LanguageUtils.Get("ServerPackWindow.Tab1.Text10"));
        Model.SetChoiseCall(_useName, Gen);
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
        var local = await PathBinding.SelectPathAsync(top, PathType.ServerPackPath);
        if (local == null)
        {
            return;
        }

        Obj.Text = Text;

        Model.Progress(LanguageUtils.Get("ServerPackWindow.Tab1.Text12"));
        var res = await Obj.GenServerPackAsync(local);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(LanguageUtils.Get("ServerPackWindow.Tab1.Text13"));
        }
        else
        {
            Model.Show(LanguageUtils.Get("ServerPackWindow.Tab1.Text11"));
        }
    }

    public override void Close()
    {
        Model.RemoveChoiseData(_useName);
        ModList.Clear();
        ResourceList.Clear();
        NameList.Clear();
    }
}
