using System.Collections.Generic;
using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 启动前检测
/// </summary>
public record LaunchCheckSetting
{
    /// <summary>
    /// 检测账户锁定
    /// </summary>
    public bool CheckUser { get; set; }
    /// <summary>
    /// 检测加载器
    /// </summary>
    public bool CheckLoader { get; set; }
    /// <summary>
    /// 检测内存大小
    /// </summary>
    public bool CheckMemory { get; set; }
}

/// <summary>
/// 样式设置
/// </summary>
public record StyleSetting
{
    /// <summary>
    /// 页面切换动画时间
    /// </summary>
    public int AmTime { get; set; }
    /// <summary>
    /// 过度淡化
    /// </summary>
    public bool AmFade { get; set; }
    /// <summary>
    /// 是否启用动画
    /// </summary>
    public bool EnableAm { get; set; }
}

/// <summary>
/// 上一个启动的账户
/// </summary>
public record LastUserSetting
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
/// 登录模型
/// </summary>
public record LockLoginSetting
{
    /// <summary>
    /// 账户类型
    /// </summary>
    public AuthType Type { get; set; }
    /// <summary>
    /// 锁定地址
    /// </summary>
    public string Data { get; set; }
    /// <summary>
    /// 登录模型名字
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// 服务器自定义
/// </summary>
public record ServerCustomSetting
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
    /// 循环播放
    /// </summary>
    public bool MusicLoop { get; set; }
    /// <summary>
    /// 游戏启动后暂停
    /// </summary>
    public bool RunPause { get; set; }

    /// <summary>
    /// 锁定登录实例
    /// </summary>
    public bool LockLogin { get; set; }
    /// <summary>
    /// 登录模型实例
    /// </summary>
    public List<LockLoginSetting> LockLogins { get; set; }
    /// <summary>
    /// 管理员方式启动
    /// </summary>
    public bool AdminLaunch { get; set; }
    /// <summary>
    /// 游戏管理员方式启动
    /// </summary>
    public bool GameAdminLaunch { get; set; }

    /// <summary>
    /// 是否使用自定义图标
    /// </summary>
    public bool CustomIcon { get; set; }
    /// <summary>
    /// 自定义图标
    /// </summary>
    public string? IconFile { get; set; }
    /// <summary>
    /// 是否使用自定义开始界面
    /// </summary>
    public bool CustomStart { get; set; }
    /// <summary>
    /// 自定义开始界面文件
    /// </summary>
    public string? StartIconFile { get; set; }
    /// <summary>
    /// 自定义开始界面显示类型
    /// </summary>
    public DisplayType DisplayType { get; set; }
    /// <summary>
    /// 自定义开始界面显示文本
    /// </summary>
    public string? StartText { get; set; }
}

/// <summary>
/// Windows窗口渲染设置
/// </summary>
public record WindowsRenderSetting
{
    public bool? ShouldRenderOnUIThread { get; set; }
    public bool? OverlayPopups { get; set; }
    public bool? Wgl { get; set; }
    public bool? UseVulkan { get; set; }
    public bool? UseSoftware { get; set; }
}

/// <summary>
/// X11窗口渲染设置
/// </summary>
public record X11RenderSetting
{
    public bool? UseDBusMenu { get; set; }
    public bool? UseDBusFilePicker { get; set; }
    public bool? OverlayPopups { get; set; }
    public bool? UseEgl { get; set; }
    public bool? UseVulkan { get; set; }
    public bool? UseSoftware { get; set; }
}

/// <summary>
/// 渲染设置
/// </summary>
public record RenderSetting
{
    /// <summary>
    /// Windows设置
    /// </summary>
    public WindowsRenderSetting Windows { get; set; }
    /// <summary>
    /// X11设置
    /// </summary>
    public X11RenderSetting X11 { get; set; }
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

public record InputSetting
{
    public bool Enable { get; set; }
    public string? NowConfig { get; set; }
}

public record HeadSetting
{
    public HeadType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

public record LogColorSetting
{
    public string Warn { get; set; }
    public string Error { get; set; }
    public string Debug { get; set; }
}

public record CardSetting
{
    public bool News { get; set; }
    public bool Online { get; set; }
    public bool Last { get; set; }
}

/// <summary>
/// Gui配置文件
/// </summary>
public record GuiConfigObj
{
    /// <summary>
    /// 使用的账户
    /// </summary>
    public LastUserSetting? LastUser { get; set; }
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
    public ServerCustomSetting ServerCustom { get; set; }
    /// <summary>
    /// 渲染设置
    /// </summary>
    public RenderSetting Render { get; set; }
    /// <summary>
    /// 日志着色
    /// </summary>
    public LogColorSetting LogColor { get; set; }
    /// <summary>
    /// 卡片展示设置
    /// </summary>
    public CardSetting Card { get; set; }

    /// <summary>
    /// 主题色类型
    /// </summary>
    public ColorType ColorType { get; set; }
    /// <summary>
    /// 主题色
    /// </summary>
    public string ColorMain { get; set; }

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
    /// 头像展示设置
    /// </summary>
    public HeadSetting Head { get; set; }
    /// <summary>
    /// Live2D设置
    /// </summary>
    public Live2DSetting Live2D { get; set; }
    /// <summary>
    /// 样式设置
    /// </summary>
    public StyleSetting Style { get; set; }
    /// <summary>
    /// 手柄绑定
    /// </summary>
    public InputSetting Input { get; set; }
    /// <summary>
    /// 启动前检测
    /// </summary>
    public LaunchCheckSetting LaunchCheck { get; set; }
    /// <summary>
    /// 服务器云同步密钥
    /// </summary>
    public string ServerKey { get; set; }

    /// <summary>
    /// 检查启动器更新
    /// </summary>
    public bool CheckUpdate { get; set; }
    /// <summary>
    /// 简易主界面
    /// </summary>
    public bool Simple { get; set; }
}

