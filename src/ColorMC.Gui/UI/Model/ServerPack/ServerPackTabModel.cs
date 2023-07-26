using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.ServerPack;

public abstract partial class ServerPackTabModel : ObservableObject
{
    protected IUserControl _con;
    public ServerPackObj Obj { get; }

    public ServerPackTabModel(IUserControl con, ServerPackObj obj)
    {
        _con = con;
        Obj = obj;
    }
}
