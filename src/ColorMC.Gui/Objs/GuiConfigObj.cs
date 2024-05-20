using System.ComponentModel;
using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 样式设置
/// </summary>
public record StyleSetting
{
    /// <summary>
    /// 按钮圆角程度
    /// </summary>
    public int ButtonCornerRadius { get; set; }
    /// <summary>
    /// 页面切换动画时间
    /// </summary>
    public int AmTime { get; set; }
    /// <summary>
    /// 过度淡化
    /// </summary>
    public bool AmFade { get; set; }
    /// <summary>
    /// 是否启用图片圆角
    /// </summary>
    public bool EnablePicRadius { get; set; }
    /// <summary>
    /// 是否启用边框圆角
    /// </summary>
    public bool EnableBorderRadius { get; set; }
}

/// <summary>
/// 主界面设置
/// </summary>
public record MainWindowSetting
{

}

/// <summary>
/// 上一个启动的账户
/// </summary>
public record LastUser
{
    /// <summary>
    /// 账户UUID
    /// </summary>
    public string UUID { get; set; }
    /// <summary>
    /// 账户类型
    /// </summary>
    public AuthType Type { get; set; }
}

/// <summary>
/// 服务器自定义
/// </summary>
public record ServerCustom
{
    /// <summary>
    /// Motd的地址
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// Motd的端口
    /// </summary>
    public ushort Port { get; set; }
    /// <summary>
    /// 是否启用Motd
    /// </summary>
    public bool Motd { get; set; }
    /// <summary>
    /// 是否自动加入服务器
    /// </summary>
    public bool JoinServer { get; set; }
    /// <summary>
    /// Motd默认字体颜色
    /// </summary>
    public string MotdColor { get; set; }
    /// <summary>
    /// Motd背景颜色
    /// </summary>
    public string MotdBackColor { get; set; }

    /// <summary>
    /// 锁定游戏实例
    /// </summary>
    public bool LockGame { get; set; }
    /// <summary>
    /// 游戏实例
    /// </summary>
    public string? GameName { get; set; }

    /// <summary>
    /// 是否启用自定义UI
    /// </summary>
    public bool EnableUI { get; set; }
    /// <summary>
    /// 自定义UI文件
    /// </summary>
    public string? UIFile { get; set; }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public bool PlayMusic { get; set; }
    /// <summary>
    /// 音乐地址
    /// </summary>
    public string? Music { get; set; }
    /// <summary>
    /// 音量大小
    /// </summary>
    public int Volume { get; set; }
    /// <summary>
    /// 缓慢提升音量
    /// </summary>
    public bool SlowVolume { get; set; }
    /// <summary>
    /// 游戏启动后暂停
    /// </summary>
    public bool RunPause { get; set; }

    /// <summary>
    /// 锁定登录实例
    /// </summary>
    public bool LockLogin { get; set; }
    /// <summary>
    /// 登录实例类型
    /// </summary>
    public int LoginType { get; set; }
    /// <summary>
    /// 登录实例网址
    /// </summary>
    public string LoginUrl { get; set; }
}

/// <summary>
/// Windows窗口渲染设置
/// </summary>
public record WindowsRender
{
    public bool? ShouldRenderOnUIThread { get; set; }
    public bool? OverlayPopups { get; set; }
}

/// <summary>
/// X11窗口渲染设置
/// </summary>
public record X11Render
{
    public bool? UseDBusMenu { get; set; }
    public bool? UseDBusFilePicker { get; set; }
    public bool? OverlayPopups { get; set; }
    public bool? SoftwareRender { get; set; }
}

/// <summary>
/// 渲染设置
/// </summary>
public record Render
{
    /// <summary>
    /// Windows设置
    /// </summary>
    public WindowsRender Windows { get; set; }
    /// <summary>
    /// X11设置
    /// </summary>
    public X11Render X11 { get; set; }
}

/// <summary>
/// 启动器颜色设置
/// </summary>
public record ColorSetting
{
    /// <summary>
    /// 背景色
    /// </summary>
    public string ColorBack { get; set; }
    /// <summary>
    /// 透明背景色
    /// </summary>
    public string ColorTranBack { get; set; }
    /// <summary>
    /// 字体颜色1
    /// </summary>
    public string ColorFont1 { get; set; }
    /// <summary>
    /// 字体颜色2
    /// </summary>
    public string ColorFont2 { get; set; }
}

public record Live2DSetting
{
    /// <summary>
    /// Live2D模型地址
    /// </summary>
    public string? Model { get; set; }
    /// <summary>
    /// 显示宽度
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// 显示高度
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; }
    /// <summary>
    /// 显示位置
    /// </summary>
    public int Pos { get; set; }
    /// <summary>
    /// 低帧率模式
    /// </summary>
    public bool LowFps { get; set; }
}

public record InputObj
{
    public bool Enable { get; set; }
    public string? NowConfig { get; set; }
}

/// <summary>
/// Gui配置文件
/// </summary>
public record GuiConfigObj
{
    /// <summary>
    /// 使用的账户
    /// </summary>
    public LastUser? LastUser { get; set; }
    /// <summary>
    /// 是否启用背景图
    /// </summary>
    public bool EnableBG { get; set; }
    /// <summary>
    /// 背景图地址
    /// </summary>
    public string? BackImage { get; set; }
    /// <summary>
    /// 背景图模糊度
    /// </summary>
    public int BackEffect { get; set; }
    /// <summary>
    /// 背景图透明度
    /// </summary>
    public int BackTran { get; set; }
    /// <summary>
    /// 背景图大小限制
    /// </summary>
    public bool BackLimit { get; set; }
    /// <summary>
    /// 背景图大小限制
    /// </summary>
    public int BackLimitValue { get; set; }
    /// <summary>
    /// 启用窗口透明
    /// </summary>
    public bool WindowTran { get; set; }
    /// <summary>
    /// 窗口透明类型
    /// </summary>
    public int WindowTranType { get; set; }

    /// <summary>
    /// 服务器设置
    /// </summary>
    public ServerCustom ServerCustom { get; set; }
    /// <summary>
    /// 渲染设置
    /// </summary>
    public Render Render { get; set; }

    /// <summary>
    /// 主题色类型
    /// </summary>
    public ColorType ColorType { get; set; }
    /// <summary>
    /// 主题色
    /// </summary>
    public string ColorMain { get; set; }

    /// <summary>
    /// 亮色
    /// </summary>
    public ColorSetting ColorLight { get; set; }
    /// <summary>
    /// 暗色
    /// </summary>
    public ColorSetting ColorDark { get; set; }

    /// <summary>
    /// 启用RGB模式
    /// </summary>
    public bool RGB { get; set; }
    /// <summary>
    /// RGB参数1
    /// </summary>
    public int RGBS { get; set; }
    /// <summary>
    /// RGB参数2
    /// </summary>
    public int RGBV { get; set; }

    /// <summary>
    /// 字体设置
    /// </summary>
    public string? FontName { get; set; }
    /// <summary>
    /// 使用默认字体
    /// </summary>
    public bool FontDefault { get; set; }
    /// <summary>
    /// 启动实例后关闭启动器
    /// </summary>
    public bool CloseBeforeLaunch { get; set; }
    /// <summary>
    /// 单窗口模式
    /// </summary>
    public bool WindowMode { get; set; }
    /// <summary>
    /// 上一次启动的游戏实例
    /// </summary>
    public string LastLaunch { get; set; }
    /// <summary>
    /// Live2D设置
    /// </summary>
    public Live2DSetting Live2D { get; set; }
    /// <summary>
    /// 主界面设置
    /// </summary>
    public MainWindowSetting Gui { get; set; }
    /// <summary>
    /// 样式设置
    /// </summary>
    public StyleSetting Style { get; set; }
    /// <summary>
    /// 手柄绑定
    /// </summary>
    public InputObj Input { get; set; }
    /// <summary>
    /// 服务器云同步密钥
    /// </summary>
    public string ServerKey { get; set; }
}

