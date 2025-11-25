using System.Collections.Concurrent;
using ColorMC.Core.Game;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Java;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.ServerPack;

namespace ColorMC.Core.Objs;

/// <summary>
/// 基础返回
/// </summary>
public record BaseRes
{
    /// <summary>
    /// 错误类型
    /// </summary>
    public ErrorType Error;
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool State;
}

/// <summary>
/// OAuth获取登陆码结果
/// </summary>
public record OAuthGetCodeRes
{
    /// <summary>
    /// 登录码
    /// </summary>
    public string Code;
    /// <summary>
    /// 登录网址
    /// </summary>
    public string Url;
    /// <summary>
    /// 设备码
    /// </summary>
    public string DeviceCode;
    /// <summary>
    /// 请求间隔
    /// </summary>
    public int ExpiresIn;
}

/// <summary>
/// OAuth Xbox live登录结果
/// </summary>
public record OAuthXboxLiveRes
{
    public string XBLToken;
    public string XBLUhs;
}

/// <summary>
/// OAuth XSTS结果
/// </summary>
public record OAuthXSTSRes
{
    public string XSTSToken;
    public string XSTSUhs;
}

/// <summary>
/// Mojang的Xbox验证结果
/// </summary>
public record MinecraftLoginRes
{
    public string AccessToken;
}

/// <summary>
/// 旧版方式登录结果
/// </summary>
public record LegacyLoginRes
{
    /// <summary>
    /// 选中的账户
    /// </summary>
    public LoginObj? Auth;
    /// <summary>
    /// 可选的账户列表
    /// </summary>
    public List<LoginObj>? Logins;
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
/// 创建一些下载项目
/// </summary>
public record MakeDownloadItemsRes : BaseRes
{
    public IEnumerable<FileItemObj>? List;
    public IDictionary<string, ModInfoObj> Mods;
}

/// <summary>
/// 获取下载项目信息
/// </summary>
public record ItemPathRes
{
    public string File;
    public string Path;
    public FileType FileType;
}

/// <summary>
/// 创建一些下载项目，附带项目名字
/// </summary>
public record MakeDownloadNameItemsRes
{
    public string Name;
    public ConcurrentBag<FileItemObj>? List;
}

/// <summary>
/// 游戏实例处理结果
/// </summary>
public record GameRes : BaseRes
{
    public GameSettingObj? Game;
}

/// <summary>
/// 消息结果
/// </summary>
public record StringRes : BaseRes
{
    public string? Data;
}

/// <summary>
/// 二进制结果
/// </summary>
public record BytesRes : BaseRes
{
    public byte[]? Data;
}

/// <summary>
/// 流结果
/// </summary>
public record StreamRes : BaseRes
{
    public Stream? Stream;
}

/// <summary>
/// 数字结果
/// </summary>
public record IntRes : BaseRes
{
    public int? Data;
}

/// <summary>
/// 游戏启动结果
/// </summary>
public record GameLaunchRes
{
    public GameHandle? Handle;
    public Exception? Exception;
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

public abstract record GetBaseRes : IDisposable
{
    public required MemoryStream Text;

    public void Dispose()
    {
        Text.Dispose();
    }
}

/// <summary>
/// 获取Fabric加载器
/// </summary>
public record GetFabricLoaderRes : GetBaseRes
{
    public FabricLoaderObj? Meta;
}

/// <summary>
/// 获取Quilt加载器
/// </summary>
public record GetQuiltLoaderRes : GetBaseRes
{
    public QuiltLoaderObj? Meta;
}

/// <summary>
/// 获取资源文件
/// </summary>
public record GetAssetsRes : GetBaseRes
{
    public AssetsObj? Assets;
}

/// <summary>
/// 获取游戏启动信息
/// </summary>
public record GetGameArgRes : GetBaseRes
{
    public GameArgObj? Arg;
}

/// <summary>
/// 获取版本文件
/// </summary>
public record GetVersionsRes : GetBaseRes
{
    public VersionObj? Version;
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
    public List<CurseForgeModObj.CurseForgeDataObj> List;
}

/// <summary>
/// 转换
/// </summary>
public record MMCToColorMCRes
{
    public GameSettingObj Game;
    public string Icon;
}

/// <summary>
/// 创建启动命令参数结果
/// </summary>
public record CreateCmdRes
{
    /// <summary>
    /// 是否创建成功
    /// </summary>
    public bool Res;
    /// <summary>
    /// 失败原因
    /// </summary>
    public string? Message;
    /// <summary>
    /// Java路径
    /// </summary>
    public string Java;
    /// <summary>
    /// 工作目录
    /// </summary>
    public string Dir;
    /// <summary>
    /// 参数列表
    /// </summary>
    public List<string> Args;
    /// <summary>
    /// 环境列表
    /// </summary>
    public Dictionary<string, string> Envs;
}

/// <summary>
/// 皮肤下载结果
/// </summary>
public record DownloadSkinRes
{
    /// <summary>
    /// 皮肤
    /// </summary>
    public string? Skin;
    /// <summary>
    /// 披风
    /// </summary>
    public string? Cape;

    /// <summary>
    /// 是否为新版纤细
    /// </summary>
    public bool IsNewSlim;
}

/// <summary>
/// Forge获取文件列表结果
/// </summary>
public record ForgeGetFilesRes
{
    /// <summary>
    /// 加载器文件列表
    /// </summary>
    public List<FileItemObj>? Loaders;
    /// <summary>
    /// 安装器文件列表
    /// </summary>
    public List<FileItemObj>? Installs;
}

/// <summary>
/// 加载器查询结果
/// </summary>
public record SupportLoaderRes
{
    /// <summary>
    /// 成功获取
    /// </summary>
    public List<Loaders> Done;
    /// <summary>
    /// 成功获取
    /// </summary>
    public List<Loaders> Fail;
}

/// <summary>
/// Jvm参数返回
/// </summary>
public record JvmArgRes
{
    /// <summary>
    /// Jvm参数
    /// </summary>
    public List<string> Args;
    /// <summary>
    /// 是否使用asm
    /// </summary>
    public bool Asm;
}