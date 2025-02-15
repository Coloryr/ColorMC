using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
    [ObservableProperty]
    private string _keyOpenFrp;

    [ObservableProperty]
    private bool _isOpenFrpEmpty = true;

    private bool _isLoadOpenFrp;

    public ObservableCollection<NetFrpRemoteModel> RemotesOpenFrp { get; set; } = [];

    private NetFrpRemoteModel _itemOpenFrp;

    partial void OnKeyOpenFrpChanged(string value)
    {
        if (_isLoadOpenFrp)
        {
            return;
        }

        ConfigBinding.SetFrpKeyOpenFrp(KeyOpenFrp);
    }

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

    [RelayCommand]
    public void OpenUrlOpenFrp()
    {
        WebBinding.OpenWeb(WebType.OpenFrp);
    }

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

    private async void LoadOpenFrpList()
    {
        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info2"));
        var res1 = await OpenFrpApi.GetChannel(KeyOpenFrp);
        Model.ProgressClose();
        if (res1 == null || res1.data == null)
        {
            return;
        }

        RemotesOpenFrp.Clear();
        foreach (var item in res1.data)
        {
            foreach (var item1 in item.proxies)
            {
                if (item1.type == "tcp")
                {
                    RemotesOpenFrp.Add(new(this, KeyOpenFrp, item, item1));
                }
            }
        }

        IsOpenFrpEmpty = RemotesOpenFrp.Count == 0;
    }

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
