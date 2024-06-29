using System.Collections.Generic;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
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
            new() { Icon = "/Resource/Icon/NetFrp/item4.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text4") },
            new() { Icon = "/Resource/Icon/NetFrp/item1.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text1") },
            new() { Icon = "/Resource/Icon/NetFrp/item5.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text5") },
            new() { Icon = "/Resource/Icon/NetFrp/item2.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text2") },
            new() { Icon = "/Resource/Icon/NetFrp/item3.svg",
                Text = App.Lang("NetFrpWindow.Tabs.Text3") }
        ]);
    }

    public async Task<bool> Open()
    {
        var user = UserBinding.GetLastUser();

        if (user?.AuthType != AuthType.OAuth)
        {
            Model.ShowOk(App.Lang("NetFrpWindow.Tab4.Error1"), WindowClose);
            return false;
        }
        Model.Progress(App.Lang("NetFrpWindow.Tab4.Info2"));
        var res = await UserBinding.TestLogin(user);
        Model.ProgressClose();
        if (!res)
        {
            Model.ShowOk(App.Lang("NetFrpWindow.Tab4.Error2"), WindowClose);
            return false;
        }

        return true;
    }

    public void RemoveClick()
    {
        Model.RemoveChoiseData(_name);
    }

    protected override void Close()
    {
        _client?.Stop();

        RemotesOpenFrp.Clear();
        RemotesSakura.Clear();
        Locals.Clear();
    }
}
