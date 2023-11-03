using Avalonia.Threading;
using ColorMC.Core.Game;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel : MenuModel
{
    private readonly LanClient _client = new();
    public ObservableCollection<NetFrpLocalModel> Locals { get; set; } = new();
    private readonly List<string> _have = new();

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
            list.Add($"樱花映射 {item.Name} {item.ID}");
        }

        var res = await Model.ShowCombo("选择通道", list);
        if (res.Cancel)
        {
            return;
        }

        var item1 = Remotes[res.Index];
        local.IsStart = true;
        var res1 = await BaseBinding.StartFrp(Key, item1, local.Motd, local.Port);
        if (!res1)
        {
            Model.Show("映射开启失败");
        }
        else
        {
            Model.Notify("映射开启成功");
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
            Locals.Add(new(this, motd, port));
        });
    }
}
