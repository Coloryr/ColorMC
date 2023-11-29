using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel(BaseModel model) : MenuModel(model)
{
    public override List<MenuObj> TabItems { get; init; } =
    [
         new() { Icon = "/Resource/Icon/NetFrp/item4.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text4") },
        new() { Icon = "/Resource/Icon/NetFrp/item1.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/NetFrp/item2.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/NetFrp/item3.svg",
            Text = App.Lang("NetFrpWindow.Tabs.Text3") }
    ];

    public async void Open()
    {
        var user = UserBinding.GetLastUser();

        if (user?.AuthType != AuthType.OAuth)
        {
            Model.ShowOk(App.Lang("NetFrpWindow.Tab4.Error1"), WindowClose);
            return;
        }
        Model.Progress(App.Lang("NetFrpWindow.Tab4.Info2"));
        var res = await UserBinding.TestLogin(user);
        Model.ProgressClose();
        if (!res)
        {
            Model.ShowOk(App.Lang("NetFrpWindow.Tab4.Error2"), WindowClose);
            return;
        }
        Load();
        LoadCloud();
    }

    protected override void Close()
    {
        _client.Stop();

        Remotes.Clear();
        Locals.Clear();
    }
}
