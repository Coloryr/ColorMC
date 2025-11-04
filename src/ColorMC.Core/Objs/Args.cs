using ColorMC.Core.GuiHandel;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.Modrinth;

namespace ColorMC.Core.Objs;

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
    public SaveObj? World;
    /// <summary>
    /// 自动加入的服务器
    /// </summary>
    public ServerObj? Server;
    /// <summary>
    /// 是否以管理员方式启动
    /// </summary>
    public bool Admin;
    /// <summary>
    /// 解密对接
    /// </summary>
    public ILauncherGui? Gui;
    /// <summary>
    /// ColorMC ASM端口
    /// </summary>
    public int? Mixinport;
}

/// <summary>
/// 导入文件夹参数
/// </summary>
public record AddGameFolderArg
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

    public ICreateInstanceGui? Gui;
}

/// <summary>
/// 复制游戏文件参数
/// </summary>
public record CopyGameFileArg
{
    /// <summary>
    /// 复制的路径
    /// </summary>
    public required string Local;
    /// <summary>
    /// 不复制的文件
    /// </summary>
    public List<string>? Unselect;
    /// <summary>
    /// 是否为根目录
    /// </summary>
    public bool IsDir;

    public ICreateInstanceGui? Gui;
}

/// <summary>
/// 解压缩包导入游戏实例参数
/// </summary>
public abstract record UnpackGameZipArg
{
    /// <summary>
    /// 实例名
    /// </summary>
    public string? Name;
    /// <summary>
    /// 添加的组
    /// </summary>
    public string? Group;

    public ICreateInstanceGui? Gui;
    public IZipGui? ZipGui;
}

/// <summary>
/// 导入整合包参数
/// </summary>
public record InstallZipArg : UnpackGameZipArg
{
    /// <summary>
    /// 压缩包位置
    /// </summary>
    public required string Dir;
    /// <summary>
    /// 整合包类型
    /// </summary>
    public required PackType Type;
}

/// <summary>
/// 安装Modrinth整合包参数
/// </summary>
public record DownloadModrinthArg : UnpackGameZipArg
{
    public required ModrinthVersionObj Data;
    public required string? IconUrl;
}

/// <summary>
/// 安装curseforge整合包参数
/// </summary>
public record DownloadCurseForgeArg : UnpackGameZipArg
{
    public required CurseForgeModObj.CurseForgeDataObj Data;
    public required string? IconUrl;
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
    /// <summary>
    /// Sha256
    /// </summary>
    public required string Sha256;
    /// <summary>
    /// 下载地址
    /// </summary>
    public required string Url;

    public IZipGui? Gui;
}

/// <summary>
/// 游戏实例导出
/// </summary>
public record GameExportArg
{
    /// <summary>
    /// 压缩包位置
    /// </summary>
    public required string File;
    /// <summary>
    /// 整合包类型
    /// </summary>
    public required PackType Type;
    /// <summary>
    /// 游戏实例
    /// </summary>
    public required GameSettingObj Obj;
    /// <summary>
    /// 选择的Mod
    /// </summary>
    public IEnumerable<ModExportObj> Mods;
    /// <summary>
    /// 选择的其他文件
    /// </summary>
    public IEnumerable<ModExportBaseObj> OtherFiles;
    /// <summary>
    /// 不选择的文件
    /// </summary>
    public List<string> UnSelectItems;
    /// <summary>
    /// 选中的文件
    /// </summary>
    public List<string> SelectItems;
    /// <summary>
    /// 名字
    /// </summary>
    public string Name;
    /// <summary>
    /// 作者
    /// </summary>
    public string Author;
    /// <summary>
    /// 版本
    /// </summary>
    public string Version;
    /// <summary>
    /// 说明
    /// </summary>
    public string Summary;
}