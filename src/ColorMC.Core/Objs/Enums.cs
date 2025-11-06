namespace ColorMC.Core.Objs;

/// <summary>
/// 加载器类型
/// </summary>
public enum Loaders
{
    /// <summary>
    /// 无Mod加载器
    /// </summary>
    Normal,
    /// <summary>
    /// Forge加载器
    /// </summary>
    Forge,
    /// <summary>
    /// Fabric加载器
    /// </summary>
    Fabric,
    /// <summary>
    /// Quilt加载器
    /// </summary>
    Quilt,
    /// <summary>
    /// NeoForge加载器
    /// </summary>
    NeoForge,
    /// <summary>
    /// 高清修复
    /// </summary>
    OptiFine,
    /// <summary>
    /// 自定义
    /// </summary>
    Custom
}

/// <summary>
/// 加载测
/// </summary>
public enum SideType
{
    /// <summary>
    /// 无法检测
    /// </summary>
    None,
    /// <summary>
    /// 客户端
    /// </summary>
    Client,
    /// <summary>
    /// 服务器
    /// </summary>
    Server,
    /// <summary>
    /// 两侧
    /// </summary>
    Both
}

/// <summary>
/// 游戏版本类型
/// </summary>
public enum GameType
{
    /// <summary>
    /// 发布版
    /// </summary>
    Release,
    /// <summary>
    /// 快照版
    /// </summary>
    Snapshot,
    /// <summary>
    /// 其他版本
    /// </summary>
    Other,
    /// <summary>
    /// 所有版本
    /// </summary>
    All
}

/// <summary>
/// 自定义加载器类型
/// </summary>
public enum CustomLoaderType
{
    /// <summary>
    /// 类Forge加载器
    /// </summary>
    ForgeLaunch
}

/// <summary>
/// 资源来源
/// </summary>
public enum SourceType
{
    CurseForge, Modrinth, McMod, ColorMC
}

/// <summary>
/// 路径类型
/// </summary>
public enum PathType
{
    BasePath, GamePath, ModPath, ConfigPath, ShaderpacksPath, ResourcepackPath, WorldBackPath,
    SavePath, SchematicsPath, ScreenshotsPath, RunPath, DownloadPath, JavaPath, PicPath, ServerPackPath, RunDir, JsonDir
}

/// <summary>
/// 文件类型
/// </summary>
public enum FileType
{
    ModPack, Mod, World, Shaderpack, Resourcepack, DataPacks, Schematic,
    Java, Game, Config, AuthConfig, Pic, Optifine, Skin, Music,
    Text, GameIcon, Head, JavaZip, Loader, InputConfig,
    User, Cmd, Icon, StartIcon, File
}

/// <summary>
/// 压缩包类型
/// </summary>
public enum PackType
{
    ColorMC, CurseForge, Modrinth, MMC, HMCL, ZipPack
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
    BMCLAPI = 1
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
/// 启动状态
/// </summary>
public enum LaunchState
{
    /// <summary>
    /// 登陆中
    /// </summary>
    Loging,
    /// <summary>
    /// 检查中
    /// </summary>
    Checking,
    /// <summary>
    /// 下载文件
    /// </summary>
    Downloading,
    /// <summary>
    /// 准备Jvm参数
    /// </summary>
    JvmPrepare,
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
    End,
    /// <summary>
    /// 下载服务器包
    /// </summary>
    LoadServerPack,
    /// <summary>
    /// 对比服务器包
    /// </summary>
    CheckServerPack,
    /// <summary>
    /// 下载服务器包所需文件
    /// </summary>
    DownloadServerPack,
    /// <summary>
    /// 服务器包文件下载完成
    /// </summary>
    DownloadServerPackDone
}

/// <summary>
/// 启动错误类型
/// </summary>
public enum LaunchError
{
    /// <summary>
    /// 检查服务器包错误
    /// </summary>
    CheckServerPackError,
    /// <summary>
    /// 检查服务器包失败
    /// </summary>
    CheckServerPackFail,
    /// <summary>
    /// 外置登录器错误
    /// </summary>
    LoginCoreError,
    /// <summary>
    /// 缺失版本文件
    /// </summary>
    LostVersionFile,
    /// <summary>
    /// 缺失加载器信息
    /// </summary>
    LostLoaderFile,
    /// <summary>
    /// 确实资源文件信息
    /// </summary>
    LostAssetsFile,
    /// <summary>
    /// 账户登录失败
    /// </summary>
    AuthLoginFail,
    /// <summary>
    /// 文件下载错误
    /// </summary>
    DownloadFileError,
    /// <summary>
    /// Java没有找到
    /// </summary>
    JavaNotFound,
    /// <summary>
    /// 启动外部程序找不到
    /// </summary>
    CmdFileNotFound,
    /// <summary>
    /// 版本号错误
    /// </summary>
    VersionError,
    /// <summary>
    /// 选中的Java没有找到
    /// </summary>
    SelectJavaNotFound,
}

/// <summary>
/// 游戏日志启动器消息
/// </summary>
public enum GameSystemLog
{
    /// <summary>
    /// 不是系统日志
    /// </summary>
    None,
    /// <summary>
    /// 运行库
    /// </summary>
    RuntimeLib,
    /// <summary>
    /// 日志重定向
    /// </summary>
    JavaRedirect,
    /// <summary>
    /// 登录用时
    /// </summary>
    LoginTime,
    /// <summary>
    /// 服务器包检查用时
    /// </summary>
    ServerPackCheckTime,
    /// <summary>
    /// 检查游戏文件用时
    /// </summary>
    CheckGameFileTime,
    /// <summary>
    /// 文件下载用时
    /// </summary>
    DownloadFileTime,
    /// <summary>
    /// 启动参数
    /// </summary>
    LaunchArgs,
    /// <summary>
    /// Java路径
    /// </summary>
    JavaPath,
    /// <summary>
    /// 启动用时
    /// </summary>
    LaunchTime,
    /// <summary>
    /// 启动前执行用时
    /// </summary>
    CmdPreTime,
    /// <summary>
    /// 启动后执行用时
    /// </summary>
    CmdPostTime,
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
    x86,
    x86_64,
    aarch64,
    arm,
    unknow
}

/// <summary>
/// 系统
/// </summary>
public enum OsType
{
    Windows,
    Linux,
    MacOS,
    Android,
    Ios
}

/// <summary>
/// 运行态
/// </summary>
public enum CoreRunState
{
    Read, Init, GetInfo, Start, End,
    Download, DownloadDone,
    Error,
}

/// <summary>
/// Dns类型
/// </summary>
public enum DnsType
{
    DnsOver, DnsOverHttps, DnsOverHttpsWithUdp
}

/// <summary>
/// 日志等级
/// </summary>
[Flags]
public enum LogLevel
{
    Base = 0b00000000,
    None = 0b00000001,
    Info = 0b00000010,
    Warn = 0b00000100,
    Error = 0b00001000,
    Debug = 0b00010000,
    All = 0b00011111,
    Fatal = 0b00100000,
}

/// <summary>
/// 编码模式
/// </summary>
public enum LogEncoding
{
    UTF8 = 0,
    GBK
}

/// <summary>
/// 下载器进度更新类型
/// </summary>
public enum UpdateType
{
    /// <summary>
    /// 添加下载项目
    /// </summary>
    AddItems,
    /// <summary>
    /// 下载项目下载结束
    /// </summary>
    ItemDone
}

/// <summary>
/// 实例修改
/// </summary>
public enum InstanceChangeType
{ 
    /// <summary>
    /// 数量修改
    /// </summary>
    NumberChange,
    /// <summary>
    /// 图标修改
    /// </summary>
    IconChange
}

/// <summary>
/// 登录错误状态
/// </summary>
public enum LoginFailState
{ 
    /// <summary>
    /// 获取数据错误
    /// </summary>
    GetOAuthCodeDataError,
    /// <summary>
    /// 获取数据为空
    /// </summary>
    GetOAuthCodeDataFail,
    /// <summary>
    /// 获取Token超时
    /// </summary>
    OAuthGetTokenTimeout,
    /// <summary>
    /// 没有选中的账户
    /// </summary>
    LoginAuthListEmpty,
    /// <summary>
    /// 密钥过期
    /// </summary>
    LoginTokenTimeout,
}

/// <summary>
/// 结构文件类型
/// </summary>
public enum SchematicType
{
    /// <summary>
    /// 原版
    /// </summary>
    Minecraft, 
    /// <summary>
    /// 投影模组
    /// </summary>
    Litematic,
    /// <summary>
    /// 创世神模组
    /// </summary>
    WorldEdit, 
    /// <summary>
    /// 机械动力蓝图
    /// </summary>
    Create
}

/// <summary>
/// 操作错误类型
/// </summary>
public enum ErrorType
{ 
    /// <summary>
    /// 文件不存在
    /// </summary>
    FileNotExist,
    /// <summary>
    /// 文件读取失败
    /// </summary>
    FileReadError,
    /// <summary>
    /// 文件下载失败
    /// </summary>
    DonwloadFail,
    /// <summary>
    /// 安装失败
    /// </summary>
    InstallFail,
    /// <summary>
    /// 解压失败
    /// </summary>
    UnzipFail,
    /// <summary>
    /// 查找文件失败
    /// </summary>
    SearchFail,
}