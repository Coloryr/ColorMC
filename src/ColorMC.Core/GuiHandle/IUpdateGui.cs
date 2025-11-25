using ColorMC.Core.Objs;

namespace ColorMC.Core.GuiHandle;

public interface IUpdateGui
{
    /// <summary>
    /// 信息更新
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="state">当前状态</param>
    public void StateUpdate(GameSettingObj obj, LaunchState state);
}
