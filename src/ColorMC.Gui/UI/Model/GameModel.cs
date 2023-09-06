using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

public abstract partial class GameModel : TopModel
{
    public GameSettingObj Obj { get; init; }

    public GameModel(BaseModel model, GameSettingObj obj) : base(model)
    {
        Obj = obj;
    }
}
