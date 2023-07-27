using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Model;

public abstract partial class GameEditModel : BaseModel
{
    public GameSettingObj Obj { get; init; }

    public GameEditModel(IUserControl con, GameSettingObj obj) : base(con)
    {
        Obj = obj;
    }
}
