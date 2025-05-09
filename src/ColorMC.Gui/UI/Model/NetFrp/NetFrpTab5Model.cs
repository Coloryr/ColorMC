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
    /// Openfrp Key
    /// </summary>
    [ObservableProperty]
    private string _keyOpenFrp;

    /// <summary>
    /// 是否没有openfrp映射
    /// </summary>
    [ObservableProperty]
    private bool _isOpenFrpEmpty = true;
    /// <summary>
    /// 是否在获取openfrp映射
    /// </summary>
    private bool _isLoadOpenFrp;

    /// <summary>
    /// openfrp列表
    /// </summary>
    public ObservableCollection<NetFrpRemoteModel> RemotesOpenFrp { get; set; } = [];

    /// <summary>
    /// 选中的openfrp项目
    /// </summary>
    private NetFrpRemoteModel? _itemOpenFrp;

    partial void OnKeyOpenFrpChanged(string value)
    {
        if (_isLoadOpenFrp)
        {
            return;
        }

        ConfigBinding.SetFrpKeyOpenFrp(KeyOpenFrp);
    }

    /// <summary>
    /// 获取openfrp项目
    /// </summary>
    [RelayCommand]
    public void GetChannelOpenFrp()
    {
        IsOpenFrpEmpty = true;

        if (string.IsNullOrWhiteSpace(KeyOpenFrp))
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error3"));
            return;
        }

        LoadOpenFrpList();
    }
    /// <summary>
    /// 打开openfrp网页
    /// </summary>
    [RelayCommand]
    public void OpenUrlOpenFrp()
    {
        WebBinding.OpenWeb(WebType.OpenFrp);
    }

    /// <summary>
    /// 加载openfep列表
    /// </summary>
    public void LoadOpenFrp()
    {
        _isLoadOpenFrp = true;

        if (FrpConfigUtils.Config.OpenFrp is { } con)
        {
            KeyOpenFrp = con.Key;
        }

        _isLoadOpenFrp = false;

        if (string.IsNullOrWhiteSpace(KeyOpenFrp))
        {
            return;
        }

        LoadOpenFrpList();
    }

    /// <summary>
    /// 加载openfep列表
    /// </summary>
    private async void LoadOpenFrpList()
    {
        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info2"));
        var res1 = await OpenFrpApi.GetChannel(KeyOpenFrp);
        Model.ProgressClose();
        if (res1 == null || res1.Data == null)
        {
            return;
        }

        _itemOpenFrp = null;
        RemotesOpenFrp.Clear();
        foreach (var item in res1.Data)
        {
            foreach (var item1 in item.Proxies)
            {
                if (item1.Type == "tcp")
                {
                    RemotesOpenFrp.Add(new(this, KeyOpenFrp, item, item1));
                }
            }
        }

        IsOpenFrpEmpty = RemotesOpenFrp.Count == 0;
    }

    /// <summary>
    /// 选中openfrp项目
    /// </summary>
    /// <param name="model"></param>
    public void SelectOpenFrp(NetFrpRemoteModel model)
    {
        if (_itemOpenFrp != null)
        {
            _itemOpenFrp.IsSelect = false;
        }

        _itemOpenFrp = model;
        model.IsSelect = true;
    }
}
