using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : TopModel
{
    [ObservableProperty]
    private string _url;
    [ObservableProperty]
    private string _version;
    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private string _uI;

    [ObservableProperty]
    private bool _forceUpdate;

    private bool _loadConfig;

    partial void OnTextChanged(string value)
    {
        SaveConfig();
    }

    partial void OnVersionChanged(string value)
    {
        SaveConfig();
    }

    partial void OnUrlChanged(string value)
    {
        SaveConfig();
    }

    partial void OnUIChanged(string value)
    {
        SaveConfig();
    }

    [RelayCommand]
    public async Task OpenUI()
    {
        var file = await PathBinding.SelectFile(FileType.UI);
        if (file == null)
            return;

        UI = file;
    }

    public void LoadConfig()
    {
        _loadConfig = true;

        Url = Obj.Url;
        Version = Obj.Version;
        Text = Obj.Text;
        UI = Obj.UI;
        ForceUpdate = Obj.ForceUpdate;

        _loadConfig = false;
    }

    private void SaveConfig()
    {
        if (_loadConfig)
            return;

        GameBinding.SaveServerPack(Obj);
    }
}
