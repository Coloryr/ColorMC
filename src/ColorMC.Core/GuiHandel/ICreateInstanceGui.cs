using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;

namespace ColorMC.Core.GuiHandel;

public interface ICreateInstanceGui
{
    /// <summary>
    /// 请求回调
    /// </summary>
    /// <returns>是否同意</returns>
    public Task<bool> InstanceNameReplace();
    /// <summary>
    /// 文件进度
    /// </summary>
    /// <param name="name">文件名</param>
    /// <param name="now">完成数量</param>
    /// <param name="count">总计数量</param>
    public void StateUpdate(string name, int now, int count);
    /// <summary>
    /// 进度
    /// </summary>
    /// <param name="now">完成数量</param>
    /// <param name="count">总计数量</param>
    public void StateUpdate(int now, int count);
    /// <summary>
    /// 游戏复写
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>是否复写</returns>
    public Task<bool> GameOverwirte(GameSettingObj obj);
    /// <summary>
    /// 安装整合包状态
    /// </summary>
    /// <param name="state">状态</param>
    public void ModPackState(CoreRunState state);
}
