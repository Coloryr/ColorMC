using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.NetFrp;

/// <summary>
/// 映射窗口
/// </summary>
public partial class NetFrpModel : MenuModel
{
    private readonly string _name;

    public NetFrpModel(BaseModel model) : base(model)
    {
        _name = ToString() ?? "NetFrpModel";

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item4.svg",
                Text = LanguageUtils.Get("NetFrpWindow.Tabs.Text4")
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item1.svg",
                Text = LanguageUtils.Get("NetFrpWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item5.svg",
                Text = LanguageUtils.Get("NetFrpWindow.Tabs.Text5")
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item6.svg",
                Text = LanguageUtils.Get("NetFrpWindow.Tabs.Text6")
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item2.svg",
                Text = LanguageUtils.Get("NetFrpWindow.Tabs.Text2"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = CleanLocal,
                        Name = LanguageUtils.Get("NetFrpWindow.Tab2.Text2")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item3.svg",
                Text = LanguageUtils.Get("NetFrpWindow.Tabs.Text3")
            }
        ]);
    }

    /// <summary>
    /// 打开窗口加载信息
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Open()
    {
        _isLoadSakura = true;

        if (FrpConfigUtils.Config.SakuraFrp is { } con)
        {
            KeySakura = con.Key;
        }

        _isLoadSakura = false;

        _isLoadOpenFrp = true;

        if (FrpConfigUtils.Config.OpenFrp is { } con1)
        {
            KeyOpenFrp = con1.Key;
        }

        _isLoadOpenFrp = false;

        if (!string.IsNullOrWhiteSpace(KeySakura))
        {
            GetChannelSakura();
        }
        if (!string.IsNullOrWhiteSpace(KeyOpenFrp))
        {
            GetChannelOpenFrp();
        }

        LoadSelfFrp();

        var list = await GameHelper.GetGameVersionsAsync(GameType.All);
        Versions.Add("");
        Versions.AddRange(list);

        return true;
    }

    /// <summary>
    /// 删除标题按钮
    /// </summary>
    public void RemoveClick()
    {
        Model.RemoveChoiseData(_name);
    }

    public override void Close()
    {
        _client?.Stop();

        RemotesOpenFrp.Clear();
        RemotesSakura.Clear();
        Locals.Clear();
    }
}
