using ColorMC.Core.Net.Apis;
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
    private string _userSakuraID;
    [ObservableProperty]
    private string _userSakuraName;

    private bool _isLoadSakura;

    public ObservableCollection<NetFrpRemoteModel> RemotesSakura { get; set; } = [];

    partial void OnKeySakuraChanged(string value)
    {
        if (_isLoadSakura)
            return;

        ConfigBinding.SetFrpKeySakura(KeySakura);
    }

    [RelayCommand]
    public async Task TestKeySakura()
    {
        if (string.IsNullOrWhiteSpace(KeySakura))
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error3"));
            return;
        }
        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info1"));
        var res = await SakuraFrpApi.GetUserInfo(KeySakura);
        Model.ProgressClose();
        if (res == null)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab1.Error1"));
            return;
        }

        UserSakuraID = res.id.ToString();
        UserSakuraName = res.name;
    }

    [RelayCommand]
    public async Task GetChannelSakura()
    {
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
    }

    [RelayCommand]
    public void OpenUrlSakura()
    {
        WebBinding.OpenWeb(WebType.NetFrpSakura);
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

        Model.Progress(App.Lang("NetFrpWindow.Tab1.Info1"));
        var res = await SakuraFrpApi.GetUserInfo(KeySakura);
        if (res == null)
        {
            Model.ProgressClose();
            return;
        }

        UserSakuraID = res.id.ToString();
        UserSakuraName = res.name;
        Model.ProgressUpdate(App.Lang("NetFrpWindow.Tab1.Info2"));
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
    }
}
