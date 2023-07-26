using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

public abstract partial class GameEditTabModel : ObservableObject
{
    protected IUserControl _con;

    public GameSettingObj Obj { get; init; }

    public GameEditTabModel(IUserControl con, GameSettingObj obj)
    {
        _con = con;
        Obj = obj;
    }
}
