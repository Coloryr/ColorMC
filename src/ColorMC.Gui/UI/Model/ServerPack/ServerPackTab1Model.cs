using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackTab1Model : ServerPackTabModel
{
    [ObservableProperty]
    private string url;
    [ObservableProperty]
    private string version;
    [ObservableProperty]
    private string text;
    [ObservableProperty]
    private string uI;

    [ObservableProperty]
    private bool forceUpdate;

    private bool load;

    public ServerPackTab1Model(IUserControl con, ServerPackObj obj) : base(con, obj)
    {

    }

    partial void OnTextChanged(string value)
    {
        Save();
    }

    partial void OnVersionChanged(string value)
    {
        Save();
    }

    partial void OnUrlChanged(string value)
    {
        Save();
    }

    partial void OnUIChanged(string value)
    {
        Save();
    }

    [RelayCommand]
    public async void OpenUI()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, Core.Objs.FileType.UI);
        if (file == null)
            return;

        UI = file;
    }

    [RelayCommand]
    public async void Gen()
    {
        var window = Con.Window;
        if (string.IsNullOrWhiteSpace(Obj.Url))
        {
            window.OkInfo.Show(App.GetLanguage("ServerPackWindow.Tab1.Error1"));
            return;
        }

        if (string.IsNullOrWhiteSpace(Obj.Version))
        {
            window.OkInfo.Show(App.GetLanguage("ServerPackWindow.Tab1.Error2"));
            return;
        }

        var local = await BaseBinding.OpPath(window, FileType.ServerPack);
        if (local == null)
            return;

        window.ProgressInfo.Show(App.GetLanguage("ServerPackWindow.Tab1.Info1"));
        var res = await GameBinding.GenServerPack(Obj, local);
        window.ProgressInfo.Close();
        if (res)
        {
            window.NotifyInfo.Show(App.GetLanguage("ServerPackWindow.Tab1.Info2"));
        }
        else
        {
            window.OkInfo.Show(App.GetLanguage("ServerPackWindow.Tab1.Error3"));
        }
    }

    public void Load()
    {
        load = true;

        Url = Obj.Url;
        Version = Obj.Version;
        Text = Obj.Text;
        UI = Obj.UI;
        ForceUpdate = Obj.ForceUpdate;

        load = false;
    }

    private void Save()
    {
        if (load)
            return;

        GameBinding.SaveServerPack(Obj);
    }
}
