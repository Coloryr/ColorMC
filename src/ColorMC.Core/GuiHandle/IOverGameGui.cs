using ColorMC.Core.Objs;

namespace ColorMC.Core.GuiHandle;

public interface IOverGameGui
{
    /// <summary>
    /// 请求修改实例名字
    /// </summary>
    /// <returns>是否同意</returns>
    public Task<bool> InstanceNameReplace();
    /// <summary>
    /// 请求实例覆盖
    /// </summary>
    /// <param name="obj">覆盖后的</param>
    /// <returns>是否覆盖</returns>
    public Task<bool> GameOverwirte(GameSettingObj obj);
}
