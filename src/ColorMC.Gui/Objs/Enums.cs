namespace ColorMC.Gui.Objs;

/// <summary>
/// 设置类型
/// </summary>
public enum SettingType
{
    /// <summary>
    /// 默认设置
    /// </summary>
    Normal,
    /// <summary>
    /// 设置Java
    /// </summary>
    SetJava,
    /// <summary>
    /// 网络设置
    /// </summary>
    Net,
    /// <summary>
    /// 启动参数
    /// </summary>
    Arg
}

/// <summary>
/// 编辑类型
/// </summary>
public enum GameEditWindowType
{
    /// <summary>
    /// 默认编辑
    /// </summary>
    Normal,
    /// <summary>
    /// 模组设置
    /// </summary>
    Mod,
    /// <summary>
    /// 世界设置
    /// </summary>
    World,
    /// <summary>
    /// 启动参数
    /// </summary>
    Arg
}

/// <summary>
/// 颜色类型
/// </summary>
public enum ColorType
{
    /// <summary>
    /// 跟随系统
    /// </summary>
    Auto,
    /// <summary>
    /// 亮色
    /// </summary>
    Light,
    /// <summary>
    /// 暗色
    /// </summary>
    Dark
}

/// <summary>
/// 运行类型
/// </summary>
public enum RunType
{
    /// <summary>
    /// 程序
    /// </summary>
    Program,
    /// <summary>
    /// 预览器
    /// </summary>
    AppBuilder,
    /// <summary>
    /// 手机
    /// </summary>
    Phone
}

/// <summary>
/// 移动方式
/// </summary>
public enum MoveType
{
    LeftUp, Up, RightUp,
    Left, Right,
    LeftDown, Down, RightDown
}

public enum WebType
{
    /// <summary>
    /// 官网
    /// </summary>
    Web,
    /// <summary>
    /// MC官网
    /// </summary>
    Minecraft,
    /// <summary>
    /// 用户手册
    /// </summary>
    Guide,
    Guide1,
    /// <summary>
    /// 赞助
    /// </summary>
    Sponsor,
    /// <summary>
    /// McMod
    /// </summary>
    Mcmod,
    /// <summary>
    /// 开源地址
    /// </summary>
    Github,
    /// <summary>
    /// 樱花映射
    /// </summary>
    SakuraFrp,
    Apache2_0,
    MIT,
    MiSans,
    BSD,
    OpenFrp,
    OpenFrpApi,
    Live2DCore,
    ColorMCDownload,
    EditSkin,
    LittleSkinEditSkin,
    UIGuide,
    UIGuide1
}

/// <summary>
/// 映射类型
/// </summary>
public enum FrpType
{
    SakuraFrp, OpenFrp, SelfFrp
}

/// <summary>
/// 头像类型
/// </summary>
public enum HeadType
{
    Head2D_A, Head3D_A, Head3D_B, Head2D_B
}

/// <summary>
/// 日志等级
/// </summary>
public enum LogLevel : int
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

public enum DisplayType
{
    /// <summary>
    /// 无动画左右
    /// </summary>
    Normal,
    /// <summary>
    /// 无动画上下
    /// </summary>
    NormalTop,
    /// <summary>
    /// 左右动画
    /// </summary>
    LeftRight,
    /// <summary>
    /// 上下动画
    /// </summary>
    TopBottom
}

public enum ModFilterType
{
    Name,
    FileName,
    Author,
    Modid,
    Enabled,
    Disabled,
    Newer
}