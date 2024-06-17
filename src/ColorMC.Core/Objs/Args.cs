using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs;

public record CoreInitArg
{
    /// <summary>
    /// 运行的路径
    /// </summary>
    public string Local;
    public string? OAuthKey;
    public string? CurseForgeKey;
}

/// <summary>
/// 开始下载所需参数
/// </summary>
public record DownloadArg
{
    /// <summary>
    /// 下载项目
    /// </summary>
    public ICollection<DownloadItemObj> List;
    /// <summary>
    /// 下载状态更新
    /// </summary>
    public ColorMCCore.DownloaderUpdate? Update;
    /// <summary>
    /// 下载项目更新
    /// </summary>
    public ColorMCCore.DownloadItemUpdate? ItemUpdate;
}

/// <summary>
/// 删除世界数据包
/// </summary>
public record DataPackDeleteArg
{
    /// <summary>
    /// 要删除的列表
    /// </summary>
    public required ICollection<DataPackObj> List;
    /// <summary>
    /// 请求回调
    /// </summary>
    public ColorMCCore.Request? Request;
}

/// <summary>
/// 删除文件夹参数
/// </summary>
public record DeleteFilesArg
{
    /// <summary>
    /// 路径
    /// </summary>
    public required string Local;
    /// <summary>
    /// 请求回调
    /// </summary>
    public ColorMCCore.Request? Request;
}

/// <summary>
/// 游戏启动所使用的参数
/// </summary>
public record GameLaunchArg
{
    /// <summary>
    /// 登录账户
    /// </summary>
    public required LoginObj Login;
    /// <summary>
    /// 自动进入的世界
    /// </summary>
    public WorldObj? World;
    public ColorMCCore.Request? Request;
    public ColorMCCore.LaunchP? Pre;
    public ColorMCCore.UpdateState? State;
    public ColorMCCore.ChoiseCall? Select;
    public ColorMCCore.NoJava? Nojava;
    public ColorMCCore.LoginFailRun? Loginfail;
    public ColorMCCore.GameLaunch? Update2;
    public int? Mixinport;
}

/// <summary>
/// 创建启动参数
/// </summary>
public record GameMakeArg
{
    /// <summary>
    /// 账户
    /// </summary>
    public required LoginObj Login;
    /// <summary>
    /// 使用的Java
    /// </summary>
    public required JavaInfo Jvm;
    /// <summary>
    /// 自动进入的世界
    /// </summary>
    public WorldObj? World;
    public int? Mixinport;
}