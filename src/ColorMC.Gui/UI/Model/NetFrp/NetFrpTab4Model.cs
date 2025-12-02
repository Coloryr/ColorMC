using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    /// <summary>
    /// 服务器映射列表
    /// </summary>
    public ObservableCollection<NetFrpCloudServerModel> CloudServers { get; init; } = [];

    /// <summary>
    /// 是否没有服务器映射
    /// </summary>
    [ObservableProperty]
    private bool _isCloudEmpty = true;

    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    private string _version;

    /// <summary>
    /// 显示信息的地址
    /// </summary>
    [ObservableProperty]
    private (string?, ushort) _iPPort;

    /// <summary>
    /// 游戏版本列表
    /// </summary>
    public List<string> Versions { get; init; } = [];

    /// <summary>
    /// 游戏版本切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnVersionChanged(string value)
    {
        GetCloud();
    }

    /// <summary>
    /// 获取服务器映射列表
    /// </summary>
    [RelayCommand]
    public void GetCloud()
    {
        IsCloudEmpty = true;
        LoadCloud();
    }

    /// <summary>
    /// 获取服务器映射列表
    /// </summary>
    public async void LoadCloud()
    {
        var dialog = Window.ShowProgress(LanguageUtils.Get("NetFrpWindow.Tab4.Text5"));
        CloudServers.Clear();
        var list = await WebBinding.GetFrpServerAsync(Version);
        Window.CloseDialog(dialog);
        if (list == null)
        {
            Window.Show(LanguageUtils.Get("NetFrpWindow.Tab4.Text9"));
            return;
        }
        foreach (var item in list)
        {
            CloudServers.Add(new(item, this));
        }

        IsCloudEmpty = CloudServers.Count == 0;
    }

    /// <summary>
    /// 获取服务器映射信息
    /// </summary>
    /// <param name="model"></param>
    public void Test(NetFrpCloudServerModel model)
    {
        IPPort = (model.IP, 0);
    }

    /// <summary>
    /// 加入该服务器映射
    /// </summary>
    /// <param name="model"></param>
    public async void Join(NetFrpCloudServerModel model)
    {
        var list = InstancesPath.Games;
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
        //选择一个游戏实例
        var dialog = new SelectModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("NetFrpWindow.Tab4.Text7"),
            Items = [.. list1]
        };
        var select = await Window.ShowDialogWait(dialog);
        if (select is not true)
        {
            return;
        }

        var item1 = list2[dialog.Index];
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
            var res = await GameBinding.LaunchAsync(item2, Window, null);
            if (!res.Res && !string.IsNullOrWhiteSpace(res.Message))
            {
                Window.Show(res.Message!);
            }
        }
        catch (Exception e)
        {
            var temp1 = LanguageUtils.Get("NetFrpWindow.Tab4.Text10");
            Logs.Error(temp1, e);
            Window.Show(temp1);
        }
    }
}
