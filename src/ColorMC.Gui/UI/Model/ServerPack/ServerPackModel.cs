using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : ServerPackTabModel
{
    public ServerPackModel(IUserControl con, ServerPackObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public async Task Gen()
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
}
