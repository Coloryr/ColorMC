using ColorMC.Core.Objs;

namespace ColorMC.Gui.UI.Model;

public abstract partial class GameModel : TopModel
{
    public GameSettingObj Obj { get; init; }

    public GameModel(BaseModel model, GameSettingObj obj) : base(model)
    {
        Obj = obj;
    }
}
