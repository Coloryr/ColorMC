using ColorMC.Core.Objs;

namespace ColorMC.Core.GuiHandle;

public interface ICreateInstanceGui : IOverGameGui
{
    /// <summary>
    /// 进度
    /// </summary>
    /// <param name="now">完成数量</param>
    /// <param name="count">总计数量</param>
    public void StateUpdate(int now, int count);
    /// <summary>
    /// 安装整合包状态
    /// </summary>
    /// <param name="state">状态</param>
    public void ModPackState(CoreRunState state);
}
