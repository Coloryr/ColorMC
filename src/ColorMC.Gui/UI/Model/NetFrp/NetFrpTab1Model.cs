using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    [ObservableProperty]
    private string _keySakura;

    [ObservableProperty]
    private bool _isSakuraEmpty = true;

    private bool _isLoadSakura;

    public ObservableCollection<NetFrpRemoteModel> RemotesSakura { get; set; } = [];

    partial void OnKeySakuraChanged(string value)
    {
        if (_isLoadSakura)
            return;

        ConfigBinding.SetFrpKeySakura(KeySakura);
    }

    [RelayCommand]
    public async Task GetChannelSakura()
    {
        IsSakuraEmpty = true;

        if (string.IsNullOrWhiteSpace(KeySakura))
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error3"));
            return;
        }
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
            RemotesSakura.Add(new(KeySakura, item));
        }

        IsSakuraEmpty = RemotesSakura.Count == 0;
    }

    [RelayCommand]
    public void OpenUrlSakura()
    {
        WebBinding.OpenWeb(WebType.SakuraFrp);
    }

    public async void LoadSakura()
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

        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info2"));
        var res1 = await SakuraFrpApi.GetChannel(KeySakura);
        Model.ProgressClose();
        if (res1 == null)
        {
            return;
        }

        RemotesSakura.Clear();
        foreach (var item in res1)
        {
            RemotesSakura.Add(new(KeySakura, item));
        }

        IsSakuraEmpty = RemotesSakura.Count == 0;
    }
}
