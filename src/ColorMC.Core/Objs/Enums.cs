namespace ColorMC.Core.Objs;

/// <summary>
/// 资源来源
/// </summary>
public enum SourceType
{
    CurseForge, Modrinth, McMod
}

/// <summary>
/// 路径类型
/// </summary>
public enum PathType
{
    BasePath, GamePath, ModPath, ConfigPath, ShaderpacksPath, ResourcepackPath
}

/// <summary>
/// 文件类型
/// </summary>
public enum FileType
{
    ModPack = 0, Mod, World, Shaderpack, Resourcepack, DataPacks, Schematic,
    Java, Game, Config, AuthConfig, Pic, UI, Optifne, Skin, ServerPack, Music,
    Text, Live2D
}

/// <summary>
/// 压缩包类型
/// </summary>
public enum PackType
{
    ColorMC, CurseForge, Modrinth, MMC, HMCL
}

/// <summary>
/// 语言
/// </summary>
public enum LanguageType
{
    zh_cn, en_us
}

/// <summary>
/// 下载源
/// </summary>
public enum SourceLocal
{
    Offical = 0,
    BMCLAPI = 1,
    MCBBS = 2
}

/// <summary>
/// 下载状态
/// </summary>
public enum GetDownloadState
{
    /// <summary>
    /// 初始化
    /// </summary>
    Init,
    /// <summary>
    /// 获取数据中
    /// </summary>
    GetInfo,
    /// <summary>
    /// 结束
    /// </summary>
    End
}

public enum AuthType
{
    /// <summary>
    /// 离线账户
    /// </summary>
    Offline,
    /// <summary>
    /// 正版登录
    /// </summary>
    OAuth,
    /// <summary>
    /// 统一通行证
    /// </summary>
    Nide8,
    /// <summary>
    /// 外置登录
    /// </summary>
    AuthlibInjector,
    /// <summary>
    /// 皮肤站
    /// </summary>
    LittleSkin,
    /// <summary>
    /// 自建皮肤站
    /// </summary>
    SelfLittleSkin
}

/// <summary>
/// 目前登录状态
/// </summary>
public enum AuthState
{
    OAuth, XBox, XSTS, Token, Profile
}

/// <summary>
/// 登录结果
/// </summary>
public enum LoginState
{
    /// <summary>
    /// 完成
    /// </summary>
    Done,
    /// <summary>
    /// 请求超时
    /// </summary>
    TimeOut,
    /// <summary>
    /// 数据错误
    /// </summary>
    JsonError,
    /// <summary>
    /// 错误
    /// </summary>
    Error,
    /// <summary>
    /// 发送崩溃
    /// </summary>
    Crash
}

/// <summary>
/// 启动状态
/// </summary>
public enum LaunchState
{
    /// <summary>
    /// 登陆中
    /// </summary>
    Login,
    /// <summary>
    /// 检查中
    /// </summary>
    Check,
    /// <summary>
    /// 检查版本文件中
    /// </summary>
    CheckVersion,
    /// <summary>
    /// 检查运行库中
    /// </summary>
    CheckLib,
    /// <summary>
    /// 检查资源文件中
    /// </summary>
    CheckAssets,
    /// <summary>
    /// 检查Mod加载器
    /// </summary>
    CheckLoader,
    /// <summary>
    /// 检查登录核心
    /// </summary>
    CheckLoginCore,
    /// <summary>
    /// 检查Mod
    /// </summary>
    CheckMods,
    /// <summary>
    /// 缺失版本文件
    /// </summary>
    LostVersion,
    /// <summary>
    /// 缺失运行库
    /// </summary>
    LostLib,
    /// <summary>
    /// 缺失加载器
    /// </summary>
    LostLoader,
    /// <summary>
    /// 缺失登陆核心
    /// </summary>
    LostLoginCore,
    /// <summary>
    /// 缺失游戏
    /// </summary>
    LostGame,
    /// <summary>
    /// 缺失文件
    /// </summary>
    LostFile,
    /// <summary>
    /// 下载文件
    /// </summary>
    Download,
    /// <summary>
    /// 下载失败
    /// </summary>
    DownloadFail,
    /// <summary>
    /// 准备Jvm参数
    /// </summary>
    JvmPrepare,
    /// <summary>
    /// 版本错误
    /// </summary>
    VersionError,
    /// <summary>
    /// 资源文件错误
    /// </summary>
    AssetsError,
    /// <summary>
    /// 加载器错误
    /// </summary>
    LoaderError,
    /// <summary>
    /// Java错误
    /// </summary>
    JavaError,
    /// <summary>
    /// 登录失败
    /// </summary>
    LoginFail,
    /// <summary>
    /// 运行前执行程序
    /// </summary>
    LaunchPre,
    /// <summary>
    /// 运行后执行程序
    /// </summary>
    LaunchPost,
    /// <summary>
    /// 结束
    /// </summary>
    End
}

/// <summary>
/// 下载状态
/// </summary>
public enum DownloadItemState
{
    /// <summary>
    /// 等待中
    /// </summary>
    Wait,
    /// <summary>
    /// 下载中
    /// </summary>
    Download,
    /// <summary>
    /// 获取信息
    /// </summary>
    GetInfo,
    /// <summary>
    /// 暂停
    /// </summary>
    Pause,
    /// <summary>
    /// 初始化中
    /// </summary>
    Init,
    /// <summary>
    /// 执行后续操作
    /// </summary>
    Action,
    /// <summary>
    /// 完成
    /// </summary>
    Done,
    /// <summary>
    /// 错误
    /// </summary>
    Error
}

/// <summary>
/// 位数
/// </summary>
public enum ArchEnum
{
    x32,
    x64,
    aarch64,
    armV7
}

/// <summary>
/// 系统
/// </summary>
public enum OsType
{
    Windows,
    Linux,
    MacOS,
    Android
}

/// <summary>
/// 运行态
/// </summary>
public enum CoreRunState
{
    Read, Init, GetInfo, Start, End,
    Download,
    Error,
}