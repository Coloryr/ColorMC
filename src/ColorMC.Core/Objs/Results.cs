using System.Collections.Concurrent;
using ColorMC.Core.Game;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Java;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;

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

/// <summary>
/// 获取服务器实例结果
/// </summary>
public record GetServerPackRes
{
    /// <summary>
    /// 服务器实例
    /// </summary>
    public ServerPackObj? Pack;
    /// <summary>
    /// Sha1
    /// </summary>
    public string? Sha1;
}

/// <summary>
/// 语言列表结果
/// </summary>
public record LangRes
{
    public string Key;
    public string Name;
}

/// <summary>
/// 创建一个下载项目结果
/// </summary>
public record MakeDownloadItemRes
{
    public bool State;
    public DownloadItemObj? Item;
}

/// <summary>
/// 创建一些下载项目
/// </summary>
public record MakeDownloadItemsRes
{
    public bool State;
    public ConcurrentBag<DownloadItemObj>? List;
}

/// <summary>
/// 创建一些下载项目，附带项目名字
/// </summary>
public record MakeDownloadNameItemsRes
{
    public string Name;
    public ConcurrentBag<DownloadItemObj>? List;
}

/// <summary>
/// 游戏实例处理结果
/// </summary>
public record GameRes
{
    public bool State;
    public GameSettingObj? Game;
}

/// <summary>
/// 消息结果
/// </summary>
public record MessageRes
{
    public bool State;
    public string? Message;
}

/// <summary>
/// 二进制结果
/// </summary>
public record BytesRes
{
    public bool State;
    public byte[]? Data;
}

/// <summary>
/// 流结果
/// </summary>
public record StreamRes
{
    public bool State;
    public Stream? Stream;
}

/// <summary>
/// 数字结果
/// </summary>
public record IntRes
{
    public bool State;
    public int? Data;
}

/// <summary>
/// 游戏启动结果
/// </summary>
public record GameLaunchRes
{
    public IGameHandel? Handel;
    public Exception? Ex;
}

/// <summary>
/// 获取OpenJ9列表
/// </summary>
public record GetOpenJ9ListRes
{
    public List<string>? Arch;
    public List<string>? Os;
    public List<string>? MainVersion;
    public List<OpenJ9FileObj.Download>? Download;
}

public record GetAssetsRes
{
    public AssetsObj Assets;
    public string Text;
}

public record GetVersionsRes
{
    public VersionObj Version;
    public string Text;
}

/// <summary>
/// 获取模组依赖列表
/// </summary>
public record GetModrinthModDependenciesRes
{
    public string Name;
    public string ModId;
    public List<ModrinthVersionObj> List;
}

/// <summary>
/// 获取模组依赖列表
/// </summary>
public record GetCurseForgeModDependenciesRes
{
    public string Name;
    public long ModId;
    public bool Opt;
    public List<CurseForgeModObj.Data> List;
}