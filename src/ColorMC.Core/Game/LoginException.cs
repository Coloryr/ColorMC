using ColorMC.Core.Objs;

namespace ColorMC.Core.Game;

/// <summary>
/// 登录错误
/// </summary>
/// <param name="fail"></param>
/// <param name="state"></param>
/// <param name="ex"></param>
/// <param name="data"></param>
public class LoginException(LoginFailState fail, AuthState state, Exception? ex = null, string? data = null) : Exception
{
    /// <summary>
    /// 登录错误状态
    /// </summary>
    public LoginFailState Fail => fail;
    /// <summary>
    /// 登录阶段
    /// </summary>
    public AuthState State => state;
    /// <summary>
    /// 内联的错误
    /// </summary>
    public Exception? Inner => ex;
    /// <summary>
    /// 原始服务器数据
    /// </summary>
    public string? Json => data;
}
