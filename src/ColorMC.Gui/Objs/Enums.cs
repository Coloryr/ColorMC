namespace ColorMC.Gui.Objs;

/// <summary>
/// 设置类型
/// </summary>
public enum SettingType
{
    Normal, SetJava
}

/// <summary>
/// 游戏编辑类型
/// </summary>
public enum GameEditWindowType
{
    Normal, Mod, Config, World, Export, Log
}

/// <summary>
/// 控件类型
/// </summary>
public enum ViewType
{
    Button, Label, ServerMotd,
    GameItem, UsearHead, StackPanel, Grid
}

public enum SkinType
{
    /// <summary>
    /// 1.7旧版
    /// </summary>
    Old,
    /// <summary>
    /// 1.8新版
    /// </summary>
    New,
    /// <summary>
    /// 1.8新版纤细
    /// </summary>
    NewSlim,
    /// <summary>
    /// 未知的类型
    /// </summary>
    Unkonw
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
    Program, AppBuilder
}