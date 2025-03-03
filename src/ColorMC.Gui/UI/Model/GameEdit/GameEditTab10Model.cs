using AvaloniaEdit.Utils;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 服务器列表
    /// </summary>
    public ObservableCollection<ServerInfoObj> ServerList { get; init; } = [];

    /// <summary>
    /// 服务器项目
    /// </summary>
    [ObservableProperty]
    private ServerInfoObj? _serverItem;
    /// <summary>
    /// 查看Motd的地址
    /// </summary>
    [ObservableProperty]
    private (string?, ushort) _iPPort;

    /// <summary>
    /// 是否没有服务器地址
    /// </summary>
    [ObservableProperty]
    private bool _serverEmptyDisplay;

    partial void OnServerItemChanged(ServerInfoObj? value)
    {
        if (value != null)
        {
            IPPort = (value?.IP, 0);
        }
    }

    /// <summary>
    /// 添加服务器地址
    /// </summary>
    private async void AddServer()
    {
        var res = await Model.InputAsync(
            App.Lang("GameEditWindow.Tab10.Info1"),
            App.Lang("GameEditWindow.Tab10.Info2"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1) || string.IsNullOrWhiteSpace(res.Text2))
        {
            Model.Show(App.Lang("GameEditWindow.Tab10.Error1"));
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab10.Info6"));
        await GameBinding.AddServer(_obj, res.Text1, res.Text2);
        Model.ProgressClose();
        Model.Notify(App.Lang("UserWindow.Info12"));
        LoadServer();
    }

    /// <summary>
    /// 加载服务器地址
    /// </summary>
    public async void LoadServer()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab10.Info4"));
        ServerList.Clear();
        ServerList.AddRange(await GameBinding.GetServers(_obj));
        Model.ProgressClose();
        ServerEmptyDisplay = ServerList.Count == 0;
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info7"));
    }

    /// <summary>
    /// 删除服务器地址
    /// </summary>
    /// <param name="obj"></param>
    public async void DeleteServer(ServerInfoObj obj)
    {
        var res = await Model.ShowAsync("GameEditWindow.Tab10.Info9");
        if (!res)
        {
            return;
        }
        Model.Progress(App.Lang("GameEditWindow.Tab10.Info6"));
        await GameBinding.DeleteServer(_obj, obj);
        Model.ProgressClose();
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info5"));
        LoadServer();
    }
}
