using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Game;

public interface ILauncherGui
{
    /// <summary>
    /// 请求回调
    /// </summary>
    /// <param name="text">显示内容</param>
    /// <returns>是否同意</returns>
    public Task<bool> Request(string text);
    /// <summary>
    /// 请求下载文件
    /// </summary>
    /// <param name="text">显示内容</param>
    /// <returns>是否同意</returns>
    public Task<bool> RequestDownload();
    /// <summary>
    /// 请求是否运行程序
    /// </summary>
    /// <param name="pre">是否为运行前启动</param>
    /// <returns>是否同意</returns>
    public Task<bool> LaunchProcess(bool pre);
    /// <summary>
    /// 启动选择框
    /// </summary>
    /// <param name="text">消息</param>
    /// <returns>是否确定</returns>
    public Task<bool> ChoiseCall(string? text);
    /// <summary>
    /// 服务器包升级
    /// </summary>
    /// <param name="text">消息</param>
    /// <returns>是否确定</returns>
    public Task<bool> ServerPackUpgrade(string text);
    /// <summary>
    /// 没有Java
    /// </summary>
    public void NoJava(int version);
    /// <summary>
    /// 登录失败是否继续运行
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns>是否继续运行</returns>
    public Task<bool> LoginFailRun(LoginObj obj);
    /// <summary>
    /// 游戏启动信息更新
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="state">当前状态</param>
    public void LaunchState(GameSettingObj obj, LaunchState state);
    /// <summary>
    /// 游戏启动错误
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="error">错误状态</param>
    public void LaunchFail(GameSettingObj obj, LaunchError error);
}
