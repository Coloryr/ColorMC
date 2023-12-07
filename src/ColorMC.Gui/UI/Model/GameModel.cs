using ColorMC.Core.Objs;

namespace ColorMC.Gui.UI.Model;

public abstract partial class GameModel(BaseModel model, GameSettingObj obj) : TopModel(model)
{
    public GameSettingObj Obj => obj;
}
