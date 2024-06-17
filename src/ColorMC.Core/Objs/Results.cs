using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Objs;

/// <summary>
/// 登录结果
/// </summary>
public record LoginRes
{
    /// <summary>
    /// 登录状态
    /// </summary>
    public AuthState AuthState;
    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginState LoginState;
    /// <summary>
    /// 账户
    /// </summary>
    public LoginObj? Auth;
    /// <summary>
    /// 消息
    /// </summary>
    public string? Message;
    /// <summary>
    /// 异常错误
    /// </summary>
    public Exception? Ex;
}

/// <summary>
/// OAuth获取登陆码结果
/// </summary>
public record OAuthGetCodeRes
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginState State;
    /// <summary>
    /// 登录码
    /// </summary>
    public string? Code;
    /// <summary>
    /// 消息
    /// </summary>
    public string? Message;
}

/// <summary>
/// OAuth获取登录返回结果
/// </summary>
public record OAuthGetTokenRes
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginState State;
    /// <summary>
    /// 信息
    /// </summary>
    public OAuthGetCodeObj? Obj;
}

/// <summary>
/// OAuth刷新密钥结果
/// </summary>
public record OAuthRefreshTokenRes : OAuthGetTokenRes
{
    
}

/// <summary>
/// OAuth Xbox live登录结果
/// </summary>
public record OAuthXboxLiveRes
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginState State;
    public string? XBLToken;
    public string? XBLUhs;
}

/// <summary>
/// OAuth XSTS结果
/// </summary>
public record OAuthXSTSRes
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginState State;
    public string? XSTSToken;
    public string? XSTSUhs;
}

/// <summary>
/// Mojang的Xbox验证结果
/// </summary>
public record MinecraftLoginRes
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginState State;
    public string? AccessToken;
}

/// <summary>
/// 旧版方式登录结果
/// </summary>
public record LegacyLoginRes
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public LoginState State;
    public LoginObj? Auth;
    public string? Message;
}