using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    public ObservableCollection<NetFrpCloudServerModel> CloudServers { get; init; } = [];

    [ObservableProperty]
    private bool _isCloudEmpty = true;

    [ObservableProperty]
    private (string?, ushort) _iPPort;

    public void GetCloud()
    {
        IsCloudEmpty = true;

        Model.HeadChoiseDisplay = false;

        LoadCloud();

        Model.HeadChoiseDisplay = true;
    }

    public async void LoadCloud()
    {
        Model.Progress(App.Lang("NetFrpWindow.Tab4.Info1"));
        CloudServers.Clear();
        var list = await WebBinding.GetCloudServer();
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab4.Error3"));
            return;
        }
        foreach (var item in list)
        {
            item.Top = this;
            CloudServers.Add(item);
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
            if (!BaseBinding.IsGameRun(item))
            {
                list1.Add(item.Name);
                list2.Add(item);
            }
        }
        var select = await Model.ShowCombo(App.Lang("NetFrpWindow.Tab4.Info3"), list1);
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
            await GameBinding.Launch(Model, item2);
        }
        catch (Exception e)
        {
            var temp1 = App.Lang("NetFrpWindow.Tab4.Error4");
            Logs.Error(temp1, e);
            Model.Show(temp1);
        }
    }
}
