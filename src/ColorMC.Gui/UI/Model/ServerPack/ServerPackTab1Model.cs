using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

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
    public async Task OpenUI()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, Core.Objs.FileType.UI);
        if (file == null)
            return;

        UI = file;
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
