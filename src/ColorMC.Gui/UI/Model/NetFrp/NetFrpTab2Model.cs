using Avalonia.Threading;
using ColorMC.Core.Game;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel : MenuModel
{
    private readonly LanClient _client = new();
    private readonly List<string> _have = new();

    public ObservableCollection<NetFrpLocalModel> Locals { get; set; } = new();

    [RelayCommand]
    public void CleanLocal()
    {
        Locals.Clear();
        _have.Clear();
    }

    public async void StartThisLan(NetFrpLocalModel local)
    {
        var list = new List<string>();
        foreach (var item in Remotes)
        {
            list.Add($"{App.Lang("NetFrpWindow.Tabs.Text1")} {item.Name} {item.ID}");
        }

        var res = await Model.ShowCombo(App.Lang("NetFrpWindow.Tab2.Info1"), list);
        if (res.Cancel)
        {
            return;
        }

        var item1 = Remotes[res.Index];
        local.IsStart = true;
        var res1 = await BaseBinding.StartFrp(Key, item1, local);
        if (!res1)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab2.Error1"));
        }
        else
        {
            Model.Notify(App.Lang("NetFrpWindow.Tab2.Info2"));
        }
    }

    public void LoadLocal()
    {
        _client.FindLan = Find;
    }

    private void Find(string motd, string ip, string port)
    {
        if (_have.Contains(ip + ":" + port))
        {
            return;
        }

        _have.Add(ip + ":" + port);

        Dispatcher.UIThread.Post(() =>
        {
            var item = new NetFrpLocalModel(this, motd, port);
            if (_isOut.Contains(port))
            {
                item.IsStart = true;
            }
            Locals.Add(item);
        });
    }
}
