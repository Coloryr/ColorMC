using System.Collections.ObjectModel;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    /// <summary>
    /// Sakura映射Key
    /// </summary>
    [ObservableProperty]
    private string _keySakura;

    /// <summary>
    /// 是否不存在Sakura映射项目
    /// </summary>
    [ObservableProperty]
    private bool _isSakuraEmpty = true;

    /// <summary>
    /// 是否在加载Sakura映射列表
    /// </summary>
    private bool _isLoadSakura;

    /// <summary>
    /// Sakura列表
    /// </summary>
    public ObservableCollection<NetFrpRemoteModel> RemotesSakura { get; set; } = [];

    /// <summary>
    /// 选中的Sakura项目
    /// </summary>
    private NetFrpRemoteModel? _itemSakura;

    partial void OnKeySakuraChanged(string value)
    {
        if (_isLoadSakura)
        {
            return;
        }

        ConfigBinding.SetFrpKeySakura(KeySakura);
    }

    /// <summary>
    /// 获取Sakura通道列表
    /// </summary>
    [RelayCommand]
    public void GetChannelSakura()
    {
        IsSakuraEmpty = true;

        if (string.IsNullOrWhiteSpace(KeySakura))
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error3"));
            return;
        }

        LoadSakuraList();
    }
    /// <summary>
    /// 打开Sakura网址
    /// </summary>
    [RelayCommand]
    public void OpenUrlSakura()
    {
        WebBinding.OpenWeb(WebType.SakuraFrp);
    }

    /// <summary>
    /// 加载Sakura列表
    /// </summary>
    public void LoadSakura()
    {
        _isLoadSakura = true;

        if (FrpConfigUtils.Config.SakuraFrp is { } con)
        {
            KeySakura = con.Key;
        }

        _isLoadSakura = false;

        if (string.IsNullOrWhiteSpace(KeySakura))
        {
            return;
        }

        LoadSakuraList();
    }

    /// <summary>
    /// 加载Sakura列表
    /// </summary>
    private async void LoadSakuraList()
    {
        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info2"));
        var res = await SakuraFrpApi.GetChannel(KeySakura);
        Model.ProgressClose();
        if (res == null)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error2"));
            return;
        }

        RemotesSakura.Clear();
        _itemSakura = null;
        foreach (var item in res)
        {
            if (item.Type == "tcp")
            {
                RemotesSakura.Add(new(this, KeySakura, item));
            }
        }

        IsSakuraEmpty = RemotesSakura.Count == 0;
    }

    /// <summary>
    /// 选择Sakura项目
    /// </summary>
    /// <param name="model"></param>
    public void SelectSakura(NetFrpRemoteModel model)
    {
        if (_itemSakura != null)
        {
            _itemSakura.IsSelect = false;
        }

        _itemSakura = model;
        model.IsSelect = true;
    }
}
