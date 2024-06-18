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
/// 下载Gui调用所需参数
/// </summary>
public record DownloadArg
{
    /// <summary>
    /// 下载状态更新
    /// </summary>
    public ColorMCCore.DownloadUpdate? Update;
    /// <summary>
    /// 下载任务更新
    /// </summary>
    public ColorMCCore.DownloadTaskUpdate? UpdateTask;
    /// <summary>
    /// 下载项目更新
    /// </summary>
    public ColorMCCore.DownloadItemUpdate? UpdateItem;
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

/// <summary>
/// 生成服务器包参数
/// </summary>
public record ServerPackGenArg
{
    /// <summary>
    /// 保存路径
    /// </summary>
    public required string Local;
    public ColorMCCore.Request? Request;
}

/// <summary>
/// 服务器包检查参数
/// </summary>
public record ServerPackCheckArg
{
    public ColorMCCore.UpdateState? State;
    public ColorMCCore.ChoiseCall? Select;
}

/// <summary>
/// 解压世界参数
/// </summary>
public record UnzipBackupWorldArg
{
    /// <summary>
    /// 文件位置
    /// </summary>
    public required string File;
    public ColorMCCore.Request Request;
}

/// <summary>
/// 导入文件夹参数
/// </summary>
public record AddGameArg
{
    /// <summary>
    /// 位置
    /// </summary>
    public required string Local;
    /// <summary>
    /// 名字
    /// </summary>
    public string Name;
    /// <summary>
    /// 不导入的文件列表
    /// </summary>
    public List<string>? Unselect;
    /// <summary>
    /// 游戏组
    /// </summary>
    public string? Group;
    public ColorMCCore.Request? Request;
    public ColorMCCore.GameOverwirte? Overwirte;
}

/// <summary>
/// 创建游戏版本参数
/// </summary>
public record CreateGameArg
{
    public required GameSettingObj Game;
    public ColorMCCore.Request? Request;
    public ColorMCCore.GameOverwirte? Overwirte;
}

/// <summary>
/// 创建游戏版本参数
/// </summary>
public record CopyGameArg
{
    public required string Game;
    public ColorMCCore.Request? Request;
    public ColorMCCore.GameOverwirte? Overwirte;
}