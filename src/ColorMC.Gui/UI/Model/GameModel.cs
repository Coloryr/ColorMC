using ColorMC.Core.Objs;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 游戏实例相关模型
/// </summary>
/// <param name="model">基础窗口</param>
/// <param name="obj">游戏实例</param>
public abstract partial class GameModel(BaseModel model, GameSettingObj obj) : TopModel(model)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Obj => obj;
}
