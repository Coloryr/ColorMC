using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackTab1Model : ServerPackBaseModel
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

    private bool _load;

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
    public async Task OpenUI()
    {
        var file = await PathBinding.SelectFile(Window, Core.Objs.FileType.UI);
        if (file == null)
            return;

        UI = file;
    }

    public void Load()
    {
        _load = true;

        Url = Obj.Url;
        Version = Obj.Version;
        Text = Obj.Text;
        UI = Obj.UI;
        ForceUpdate = Obj.ForceUpdate;

        _load = false;
    }

    private void Save()
    {
        if (_load)
            return;

        GameBinding.SaveServerPack(Obj);
    }
}
