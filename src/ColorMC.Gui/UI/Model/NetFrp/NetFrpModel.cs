using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.Frp;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.NetFrp;

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
                Text = App.Lang("NetFrpWindow.Tabs.Text4")
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item1.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item5.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text5")
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item2.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text2"),
                SubMenu =
                [
                    new SubMenuItemModel()
                    {
                        Func = CleanLocal,
                        Name = App.Lang("NetFrpWindow.Tab2.Text2")
                    }
                ]
            },
            new()
            {
                Icon = "/Resource/Icon/NetFrp/item3.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text3")
            }
        ]);
    }

    public async Task<bool> Open()
    {
        _isLoadSakura = true;

        if (FrpConfig.Config.SakuraFrp is { } con)
        {
            KeySakura = con.Key;
        }

        _isLoadSakura = false;

        _isLoadOpenFrp = true;

        if (FrpConfig.Config.OpenFrp is { } con1)
        {
            KeyOpenFrp = con1.Key;
        }

        _isLoadOpenFrp = false;

        if (!string.IsNullOrWhiteSpace(KeySakura))
        {
            await GetChannelSakura();
        }
        if (!string.IsNullOrWhiteSpace(KeyOpenFrp))
        {
            await GetChannelOpenFrp();
        }

        var list = await GameBinding.GetGameVersions(GameType.All);
        Versions.Add("");
        Versions.AddRange(list);

        return true;
    }

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
