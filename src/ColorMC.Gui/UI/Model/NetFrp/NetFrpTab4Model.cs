using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    public ObservableCollection<NetFrpCloudServerModel> CloudServers { get; init; } = [];

    [ObservableProperty]
    private bool _isCloudEmpty = true;

    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private (string?, ushort) _iPPort;

    public List<string> Versions { get; init; } = [];

    partial void OnVersionChanged(string value)
    {
        GetCloud();
    }

    [RelayCommand]
    public void GetCloud()
    {
        IsCloudEmpty = true;
        LoadCloud();
    }

    public async void LoadCloud()
    {
        Model.Progress(App.Lang("NetFrpWindow.Tab4.Info1"));
        CloudServers.Clear();
        var list = await WebBinding.GetFrpServer(Version);
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab4.Error3"));
            return;
        }
        foreach (var item in list)
        {
            CloudServers.Add(new(item)
            {
                TopModel = this
            });
        }

        IsCloudEmpty = CloudServers.Count == 0;
    }

    public void Test(NetFrpCloudServerModel model)
    {
        IPPort = (model.IP, 0);
    }

    public async void Join(NetFrpCloudServerModel model)
    {
        var list = GameBinding.GetGames();
        var list1 = new List<string>();
        var list2 = new List<GameSettingObj>();
        foreach (var item in list)
        {
            if (!GameManager.IsGameRun(item))
            {
                list1.Add(item.Name);
                list2.Add(item);
            }
        }
        var select = await Model.Combo(App.Lang("NetFrpWindow.Tab4.Info3"), list1);
        if (select.Cancel)
        {
            return;
        }

        var item1 = list2[select.Index];
        var item2 = item1.CopyObj();
        item2.UUID = item1.UUID;
        item2.LaunchData = item1.LaunchData;
        try
        {
            var temp = model.IP.Split(':');
            item2.StartServer = new()
            {
                IP = temp[0],
                Port = ushort.Parse(temp[1])
            };
            var res = await GameBinding.Launch(Model, item2);
            if (!res.Res && !string.IsNullOrWhiteSpace(res.Message))
            {
                Model.Show(res.Message!);
            }
        }
        catch (Exception e)
        {
            var temp1 = App.Lang("NetFrpWindow.Tab4.Error4");
            Logs.Error(temp1, e);
            Model.Show(temp1);
        }
    }
}
