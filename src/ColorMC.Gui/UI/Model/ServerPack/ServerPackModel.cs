using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using Esprima.Ast;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : TopModel
{
    public ServerPackObj Obj { get; }

    public ServerPackModel(BaseModel model, ServerPackObj obj) : base(model)
    {
        Obj = obj;
    }

    [RelayCommand]
    public async Task Gen()
    {
        if (string.IsNullOrWhiteSpace(Obj.Url))
        {
            Model.Show(App.GetLanguage("ServerPackWindow.Tab1.Error1"));
            return;
        }

        if (string.IsNullOrWhiteSpace(Obj.Version))
        {
            Model.Show(App.GetLanguage("ServerPackWindow.Tab1.Error2"));
            return;
        }

        var local = await PathBinding.SelectPath(FileType.ServerPack);
        if (local == null)
            return;

        Model.Progress(App.GetLanguage("ServerPackWindow.Tab1.Info1"));
        var res = await GameBinding.GenServerPack(Obj, local);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.GetLanguage("ServerPackWindow.Tab1.Info2"));
        }
        else
        {
            Model.Show(App.GetLanguage("ServerPackWindow.Tab1.Error3"));
        }
    }

    protected override void Close()
    {
        ModList.Clear();
        ConfigList.Clear();
        NameList.Clear();
    }
}
