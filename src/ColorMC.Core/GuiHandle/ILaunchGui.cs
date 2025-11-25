using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.GuiHandle;

public interface ILaunchGui : IUpdateGui
{
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
    public Task<bool> LoginFail(LoginObj obj);

}
