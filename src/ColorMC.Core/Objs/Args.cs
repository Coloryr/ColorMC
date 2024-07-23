using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;

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
    public required LoginObj Auth;
    /// <summary>
    /// 自动进入的世界
    /// </summary>
    public WorldObj? World;
    public ServerObj? Server;
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
    public ServerObj? Server;
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
    public string? Name;
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

public abstract record UnpackGameZipArg
{
    public string? Name;
    public string? Group;
    public ColorMCCore.ZipUpdate? Zip;
    public ColorMCCore.Request? Request;
    public ColorMCCore.GameOverwirte? Overwirte;
    public ColorMCCore.PackUpdate? Update;
    public ColorMCCore.PackState? Update2;
}

/// <summary>
/// 导入整合包参数
/// </summary>
public record InstallZipArg : UnpackGameZipArg
{
    public required string Dir;
    public required PackType Type;
}

/// <summary>
/// 安装Modrinth整合包参数
/// </summary>
public record DownloadModrinthArg : UnpackGameZipArg
{
    public required ModrinthVersionObj Data;
    public required ModrinthSearchObj.Hit Data1;
}

/// <summary>
/// 安装curseforge整合包参数
/// </summary>
public record DownloadCurseForgeArg : UnpackGameZipArg
{
    public required CurseForgeModObj.Data Data;
    public required CurseForgeObjList.Data Data1;
}

/// <summary>
/// 安装压缩包参数
/// </summary>
public record InstallModPackZipArg
{
    public required string Zip;
    public string? Name;
    public string? Group;
    public ColorMCCore.Request? Request;
    public ColorMCCore.GameOverwirte? Overwirte;
    public ColorMCCore.PackUpdate? Update;
    public ColorMCCore.PackState? Update2;
}

/// <summary>
/// 获取CurseForge整合包Mod信息参数
/// </summary>
public record GetCurseForgeModInfoArg
{
    public required GameSettingObj Game;
    public required CurseForgePackObj Info;
    public ColorMCCore.PackUpdate? Update;
}

/// <summary>
/// 获取Modrinth整合包Mod信息参数
/// </summary>
public record GetModrinthModInfoArg
{
    public required GameSettingObj Game;
    public required ModrinthPackObj Info;
    public ColorMCCore.PackUpdate? Update;
}

/// <summary>
/// 升级整合包参数
/// </summary>
public record UpdateModPackArg
{
    public required GameSettingObj Game;
    public required string Zip;
    public ColorMCCore.PackUpdate? Update;
    public ColorMCCore.PackState? Update2;
}

/// <summary>
/// 更新CurseForge整合包
/// </summary>
public record UpdateCurseForgeModPackArg
{
    public required GameSettingObj Game;
    public required CurseForgeModObj.Data Data;
    public ColorMCCore.PackUpdate Update;
    public ColorMCCore.PackState Update2;
}

/// <summary>
/// 更新Modrinth整合包
/// </summary>
public record UpdateModrinthModPackArg
{
    public required GameSettingObj Game;
    public required ModrinthVersionObj Data;
    public ColorMCCore.PackUpdate Update;
    public ColorMCCore.PackState Update2;
}

/// <summary>
/// 安装Java参数
/// </summary>
public record InstallJvmArg
{
    /// <summary>
    /// 文件名
    /// </summary>
    public required string File;
    /// <summary>
    /// 名字
    /// </summary>
    public required string Name;
    public required string Sha256;
    /// <summary>
    /// 下载地址
    /// </summary>
    public required string Url;
    public ColorMCCore.ZipUpdate? Zip;
    public ColorMCCore.JavaUnzip? Unzip;
}

/// <summary>
/// 解压参数
/// </summary>
public record UnzipArg
{
    /// <summary>
    /// 名字
    /// </summary>
    public required string Name;
    /// <summary>
    /// 文件
    /// </summary>
    public required string File;
    public ColorMCCore.ZipUpdate? Zip;
}