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
    Net
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
    World
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

public enum FrpType
{
    SakuraFrp, OpenFrp
}

public enum HeadType
{
    Head2D, Head3D_A, Head3D_B
}