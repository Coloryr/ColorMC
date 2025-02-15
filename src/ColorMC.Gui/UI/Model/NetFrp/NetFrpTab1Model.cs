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
    private string _keySakura;

    [ObservableProperty]
    private bool _isSakuraEmpty = true;

    private bool _isLoadSakura;

    public ObservableCollection<NetFrpRemoteModel> RemotesSakura { get; set; } = [];

    private NetFrpRemoteModel? _itemSakura;

    partial void OnKeySakuraChanged(string value)
    {
        if (_isLoadSakura)
        {
            return;
        }

        ConfigBinding.SetFrpKeySakura(KeySakura);
    }

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

    [RelayCommand]
    public void OpenUrlSakura()
    {
        WebBinding.OpenWeb(WebType.SakuraFrp);
    }

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
        foreach (var item in res)
        {
            if (item.type == "tcp")
            {
                RemotesSakura.Add(new(this, KeySakura, item));
            }
        }

        IsSakuraEmpty = RemotesSakura.Count == 0;
    }

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
