using Avalonia.Threading;
using ColorMC.Core.Game;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    private LanClient _client;
    private readonly List<string> _have = [];

    [ObservableProperty]
    private bool _isLocalEmpty = true;

    public ObservableCollection<NetFrpLocalModel> Locals { get; set; } = [];

    [RelayCommand]
    public void CleanLocal()
    {
        Locals.Clear();
        _have.Clear();

        IsLocalEmpty = true;
    }

    public async void StartThisLan(NetFrpLocalModel local)
    {
        if (RemotesSakura.Count == 0 && RemotesOpenFrp.Count == 0)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab2.Error2"));
            return;
        }
        var list = new List<string>();
        var list1 = new List<NetFrpRemoteModel>();
        foreach (var item in RemotesSakura)
        {
            list1.Add(item);
            list.Add($"{App.Lang("NetFrpWindow.Tabs.Text1")} {item.Name} {item.ID}");
        }

        foreach (var item in RemotesOpenFrp)
        {
            list1.Add(item);
            list.Add($"{App.Lang("NetFrpWindow.Tabs.Text5")} {item.Name} {item.ID}");
        }

        var (Cancel, Index, _) = await Model.ShowCombo(App.Lang("NetFrpWindow.Tab2.Info1"), list);
        if (Cancel)
        {
            return;
        }

        var item1 = list1[Index];
        local.IsStart = true;
        var res1 = await BaseBinding.StartFrp(item1, local);
        if (!res1.Item1)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab2.Error1"));
        }
        else
        {
            local.IsStart = false;
            SetProcess(res1.Item2!, local, res1.Item3!);
            Model.Notify(App.Lang("NetFrpWindow.Tab2.Info2"));
            NowView = 4;
        }
    }

    public void LoadLocal()
    {
        _client ??= new()
        {
            FindLan = Find
        };
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
            IsLocalEmpty = false;
            var item = new NetFrpLocalModel(this, motd, port);
            if (_isOut.Contains(port))
            {
                item.IsStart = true;
            }
            Locals.Add(item);
        });
    }

    public void SetTab2Click()
    {
        Model.SetChoiseCall(_name, CleanLocal);
        Model.SetChoiseContent(_name, App.Lang("NetFrpWindow.Tab2.Text2"));
    }
}
