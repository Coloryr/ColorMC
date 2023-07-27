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
        if (string.IsNullOrWhiteSpace(Obj.Url))
        {
            Show(App.GetLanguage("ServerPackWindow.Tab1.Error1"));
            return;
        }

        if (string.IsNullOrWhiteSpace(Obj.Version))
        {
            Show(App.GetLanguage("ServerPackWindow.Tab1.Error2"));
            return;
        }

        var local = await BaseBinding.OpPath(Window, FileType.ServerPack);
        if (local == null)
            return;

        Progress(App.GetLanguage("ServerPackWindow.Tab1.Info1"));
        var res = await GameBinding.GenServerPack(Obj, local);
        ProgressClose();
        if (res)
        {
            Notify(App.GetLanguage("ServerPackWindow.Tab1.Info2"));
        }
        else
        {
            Show(App.GetLanguage("ServerPackWindow.Tab1.Error3"));
        }
    }
}
