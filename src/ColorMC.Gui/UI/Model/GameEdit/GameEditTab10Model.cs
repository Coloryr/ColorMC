using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

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
        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LanguageUtils.Get("GameEditWindow.Tab10.Text4"),
            Watermark2 = LanguageUtils.Get("Text.ServerAddress"),
            Text2Visable = true
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(dialog.Text1) || string.IsNullOrWhiteSpace(dialog.Text2))
        {
            Window.Show(LanguageUtils.Get("GameEditWindow.Tab10.Text11"));
            return;
        }

        var dialog1 = Window.ShowProgress(LanguageUtils.Get("GameEditWindow.Tab10.Text7"));
        var res1 = await _obj.AddServerAsync(dialog.Text1, dialog.Text2);
        Window.CloseDialog(dialog1);
        if (res1)
        {
            Window.Notify(LanguageUtils.Get("GameEditWindow.Tab10.Text10"));
            LoadServer();
        }
        else
        {
            Window.Show(LanguageUtils.Get("GameEditWindow.Tab10.Text12"));
        }
    }

    /// <summary>
    /// 加载服务器地址
    /// </summary>
    public async void LoadServer()
    {
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameEditWindow.Tab10.Text5"));
        ServerList.Clear();
        ServerList.AddRange(await _obj.GetServerInfosAsync());
        Window.CloseDialog(dialog);
        ServerEmptyDisplay = ServerList.Count == 0;
        Window.Notify(LanguageUtils.Get("GameEditWindow.Tab10.Text8"));
    }

    /// <summary>
    /// 删除服务器地址
    /// </summary>
    /// <param name="obj"></param>
    public async void DeleteServer(ServerInfoObj obj)
    {
        var res = await Window.ShowChoice("GameEditWindow.Tab10.Text9");
        if (!res)
        {
            return;
        }
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameEditWindow.Tab10.Text7"));
        res = await obj.DeleteAsync();
        Window.CloseDialog(dialog);
        if (res)
        {
            Window.Notify(LanguageUtils.Get("GameEditWindow.Tab10.Text6"));
            LoadServer();
        }
        else
        {
            Window.Show(LanguageUtils.Get("GameEditWindow.Tab10.Text13"));
        }
    }
}
