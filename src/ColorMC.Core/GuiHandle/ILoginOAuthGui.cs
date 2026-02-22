using ColorMC.Core.Objs;

namespace ColorMC.Core.GuiHandle;

public interface ILoginOAuthGui
{
    /// <summary>
    /// OAuth登录
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="code">登陆码</param>
    public void LoginOAuthCode(string? url, string code);
    /// <summary>
    /// 状态修改
    /// </summary>
    /// <param name="state">登录状态</param>
    public void LoginOAuthState(AuthState state);
    /// <summary>
    /// 结束显示
    /// </summary>
    public void Close();
}
