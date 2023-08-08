using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Model;

public abstract partial class GameModel : BaseModel
{
    public GameSettingObj Obj { get; init; }

    public GameModel(IUserControl con, GameSettingObj obj) : base(con)
    {
        Obj = obj;
    }
}
