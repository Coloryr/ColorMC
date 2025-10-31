using ColorMC.Core.Downloader;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Objs;

/// <summary>
/// 错误事件
/// </summary>
/// <param name="show">是否展示窗口显示</param>
/// <param name="close">是否同时关闭程序</param>
public abstract class CoreErrorEventArgs(bool show, bool close) : EventArgs
{
    /// <summary>
    /// 是否同时关闭程序
    /// </summary>
    public bool Close => close;
    /// <summary>
    /// 是否展示窗口显示
    /// </summary>
    public bool Show => show;
}

/// <summary>
/// 报错事件
/// </summary>
/// <param name="exception">错误</param>
/// <param name="show">是否展示窗口显示</param>
/// <param name="close">是否同时关闭程序</param>
public abstract class ExceptionErrorEventArgs(Exception exception, bool show, bool close) : CoreErrorEventArgs(show, close)
{
    /// <summary>
    /// 错误内容
    /// </summary>
    public Exception Exception => exception;
}

/// <summary>
/// 配置文件加载错误
/// </summary>
/// <param name="exception">错误</param>
public class ConfigLoadErrorEventArgs(Exception exception) : ExceptionErrorEventArgs(exception, true, false)
{

}

/// <summary>
/// 配置文件保存错误
/// </summary>
/// <param name="exception">错误</param>
public class ConfigSaveErrorEventArgs(Exception exception) : ExceptionErrorEventArgs(exception, true, false)
{

}

/// <summary>
/// 下载文件大小错误
/// </summary>
/// <param name="file">下载的文件</param>
/// <param name="size">总共下载大小</param>
public class DownloadSizeErrorEvnetArgs(FileItemObj file, long size) : CoreErrorEventArgs(false, false)
{
    /// <summary>
    /// 下载项目
    /// </summary>
    public FileItemObj File => file;
    /// <summary>
    /// 总计下载大小
    /// </summary>
    public long Size => size;
}

/// <summary>
/// 下载文件校验失败
/// </summary>
/// <param name="file">下载的文件</param>
/// <param name="hash">预计校验值</param>
/// <param name="now">实际校验值</param>
public class DownloadHashErrorEventArgs(FileItemObj file, string hash, string now) : CoreErrorEventArgs(false, false)
{
    /// <summary>
    /// 下载项目
    /// </summary>
    public FileItemObj File => file;
    /// <summary>
    /// 预计校验值
    /// </summary>
    public string Hash => hash;
    /// <summary>
    /// 实际校验值
    /// </summary>
    public string Now => now;
}

/// <summary>
/// 游戏日志
/// </summary>
/// <param name="game">游戏实例</param>
/// <param name="log">日志内容</param>
public class GameLogEventArgs(GameSettingObj game, GameLogItemObj? log) : EventArgs
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 日志内容
    /// </summary>
    public GameLogItemObj? Log => log;
}

/// <summary>
/// 游戏实例退出事件
/// </summary>
/// <param name="game">游戏实例</param>
/// <param name="login">登录账户</param>
/// <param name="code">退出代码</param>
public class GameExitEventArgs(GameSettingObj game, LoginObj login, int code) : EventArgs
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 登录账户
    /// </summary>
    public LoginObj Login => login;
    /// <summary>
    /// 退出代码
    /// </summary>
    public int Code => code;
}

/// <summary>
/// 游戏实例修改事件
/// </summary>
/// <param name="game"></param>
public class InstanceChangeEventArgs(InstanceChangeType type, GameSettingObj? game = null) : EventArgs
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public InstanceChangeType Type => type;
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj? Game => game;
}

/// <summary>
/// 开始下载回调
/// </summary>
public class DownloadEventArgs : EventArgs
{
    /// <summary>
    /// 界面操作回调
    /// </summary>
    public IDownloadGuiHandel? GuiHandel { get; set; }
}