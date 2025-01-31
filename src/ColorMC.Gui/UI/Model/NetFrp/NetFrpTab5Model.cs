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

    partial void OnKeyOpenFrpChanged(string value)
    {
        if (_isLoadOpenFrp)
            return;

        ConfigBinding.SetFrpKeyOpenFrp(KeyOpenFrp);
    }

    [RelayCommand]
    public async Task GetChannelOpenFrp()
    {
        IsOpenFrpEmpty = true;

        if (string.IsNullOrWhiteSpace(KeyOpenFrp))
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error3"));
            return;
        }
        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info2"));
        var res = await OpenFrpApi.GetChannel(KeyOpenFrp);
        Model.ProgressClose();
        if (res == null || res.data == null)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error2"));
            return;
        }

        RemotesOpenFrp.Clear();
        foreach (var item in res.data)
        {
            foreach (var item1 in item.proxies)
            {
                RemotesOpenFrp.Add(new(KeyOpenFrp, item, item1));
            }
        }

        IsOpenFrpEmpty = RemotesOpenFrp.Count == 0;
    }

    [RelayCommand]
    public void OpenUrlOpenFrp()
    {
        WebBinding.OpenWeb(WebType.OpenFrp);
    }

    public async void LoadOpenFrp()
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

        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info1"));
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
                RemotesOpenFrp.Add(new(KeyOpenFrp, item, item1));
            }
        }

        IsOpenFrpEmpty = RemotesOpenFrp.Count == 0;
    }
}
