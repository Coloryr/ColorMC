using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using ColorMC.Core.Game;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    /// <summary>
    /// 本地游戏检测
    /// </summary>
    private LanClient _client;
    /// <summary>
    /// 已有的本地游戏列表
    /// </summary>
    private readonly List<string> _have = [];

    /// <summary>
    /// 是否没有本地游戏列表
    /// </summary>
    [ObservableProperty]
    private bool _isLocalEmpty = true;

    /// <summary>
    /// 本地游戏列表
    /// </summary>
    public ObservableCollection<NetFrpLocalModel> Locals { get; set; } = [];

    /// <summary>
    /// 选中的本地游戏
    /// </summary>
    private NetFrpLocalModel? _localItem;

    /// <summary>
    /// 清理本地游戏列表
    /// </summary>
    [RelayCommand]
    public void CleanLocal()
    {
        Locals.Clear();
        _have.Clear();

        _localItem = null;
        IsLocalEmpty = true;
    }

    /// <summary>
    /// 开始映射该本地游戏
    /// </summary>
    /// <param name="local">本地游戏地址</param>
    public async void StartThisFrp(NetFrpLocalModel local)
    {
        if (RemotesSakura.Count == 0 && RemotesOpenFrp.Count == 0)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab2.Error2"));
            return;
        }
        var list = new List<string>();
        var list1 = new List<object>();
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

        foreach (var item in RemoteSelfFrp)
        {
            list1.Add(item);
            list.Add($"{App.Lang("NetFrpWindow.Tabs.Text6")} {item.Name} {item.IP}");
        }

        var res = await Model.Combo(App.Lang("NetFrpWindow.Tab2.Info1"), list);
        if (res.Cancel)
        {
            return;
        }

        //选择一个通道
        var item1 = list1[res.Index];
        var res1 = await BaseBinding.StartFrp(item1, local);
        if (!res1.Res)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab2.Error1"));
        }
        else
        {
            local.IsStart = true;
            SetProcess(res1.Process!, local, res1.IP!);
            Model.Notify(App.Lang("NetFrpWindow.Tab2.Info2"));
            NowView = 5;
        }
    }

    /// <summary>
    /// 加载本地游戏
    /// </summary>
    public void LoadLocal()
    {
        _client ??= new()
        {
            FindLan = Find
        };
    }

    /// <summary>
    /// 发现本地游戏
    /// </summary>
    /// <param name="motd">显示内容</param>
    /// <param name="ip">地址</param>
    /// <param name="port">端口</param>
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

    /// <summary>
    /// 选中本地游戏项目
    /// </summary>
    /// <param name="model"></param>
    public void SelectLocal(NetFrpLocalModel model)
    {
        if (_localItem != null)
        {
            _localItem.IsSelect = false;
        }

        model.IsSelect = true;
        _localItem = model;
    }
}
