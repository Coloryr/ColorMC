using ColorMC.Core.Downloader;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;

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
/// 下载报错
/// </summary>
/// <param name="file">下载的文件</param>
/// <param name="exception">错误</param>
public class DownloadExceptionErrorEventArgs(FileItemObj file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 下载项目
    /// </summary>
    public FileItemObj File => file;
}

/// <summary>
/// 游戏语言读取错误
/// </summary>
/// <param name="key">语言键</param>
/// <param name="exception">错误</param>
public class GameLangLoadErrorEventArgs(string key, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 语言键
    /// </summary>
    public string Key => key;
}

/// <summary>
/// 游戏实例服务器包生成失败
/// </summary>
/// <param name="game">游戏实例</param>
/// <param name="exception">错误</param>
public class GameServerPackErrorEventArgs(GameSettingObj game, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
}

/// <summary>
/// 日志文件读取错误
/// </summary>
/// <param name="game">游戏实例</param>
/// <param name="file">日志文件</param>
/// <param name="exception">错误</param>
public class GameLogFileErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 日志文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 模组读取错误
/// </summary>
/// <param name="game">游戏实例</param>
/// <param name="file">模组文件</param>
/// <param name="exception">错误</param>
public class GameModErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 添加模组出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameModAddErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 读取模组出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameModReadErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 读取材质包出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameResourcepackReadErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 添加材质包出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameResourcepackAddErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 导入存档出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameSaveAddErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 还原存档出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameSaveRestoreErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 读取存档出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameSaveReadErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 导入结构文件出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameSchematicAddErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 导入结构读取出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameSchematicReadErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 导入结构读取出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="exception"></param>
public class GameServerReadErrorEventArgs(GameSettingObj game,  Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
}

/// <summary>
/// 添加服务器时错误
/// </summary>
/// <param name="game"></param>
/// <param name="name"></param>
/// <param name="ip"></param>
/// <param name="exception"></param>
public class GameServerAddErrorEventArgs(GameSettingObj game, string name, string ip, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 服务器名字
    /// </summary>
    public string Name => name;
    /// <summary>
    /// 地址
    /// </summary>
    public string IP => ip;
}

/// <summary>
/// 删除服务器时错误
/// </summary>
/// <param name="server"></param>
/// <param name="exception"></param>
public class GameServerDeleteErrorEventArgs(ServerInfoObj server, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 服务器信息
    /// </summary>
    public ServerInfoObj Server => server;
}

/// <summary>
/// 光影包添加出现错误
/// </summary>
/// <param name="game"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameShaderpackAddErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 数据包读取出现错误
/// </summary>
/// <param name="save"></param>
/// <param name="file"></param>
/// <param name="exception"></param>
public class GameDataPackReadErrorEventArgs(SaveObj save, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 存档
    /// </summary>
    public SaveObj Save => save;
    /// <summary>
    /// 模组文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// 删除数据包出现错误
/// </summary>
/// <param name="save"></param>
/// <param name="exception"></param>
public class GameDataPackDeleteErrorEventArgs(SaveObj save, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 存档
    /// </summary>
    public SaveObj Save => save;
}

/// <summary>
/// 安装整合包出现错误
/// </summary>
/// <param name="file"></param>
/// <param name="exception"></param>
public class InstallModPackErrorEventArgs(string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 名字
    /// </summary>
    public string File => file;
}

/// <summary>
/// Java检查时错误
/// </summary>
/// <param name="java"></param>
/// <param name="exception"></param>
public class CheckJavaErrorEventArgs(string java, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// Java文件路径
    /// </summary>
    public string Java => java;
}

/// <summary>
/// Java检查时错误
/// </summary>
/// <param name="exception"></param>
public class ScanJavaErrorEventArgs(Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{

}

/// <summary>
/// 读取实例时错误
/// </summary>
/// <param name="path"></param>
/// <param name="exception"></param>
public class InstanceLoadErrorEventArgs(string path, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 实例路径
    /// </summary>
    public string Path => path;
}

/// <summary>
/// 创建实例时错误
/// </summary>
/// <param name="name"></param>
/// <param name="exception"></param>
public class InstanceCreateErrorEventArgs(string name, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 实例名字
    /// </summary>
    public string Name => name;
}

/// <summary>
/// 游戏实例读取模组信息时错误
/// </summary>
/// <param name="game"></param>
/// <param name="exception"></param>
public class InstanceReadModErrorEventArgs(GameSettingObj game, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
}

/// <summary>
/// 读取游戏实例统计数据错误
/// </summary>
/// <param name="game"></param>
/// <param name="exception"></param>
public class InstanceReadCountErrorEventArgs(GameSettingObj game, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
}

/// <summary>
/// 游戏实例设置图标错误
/// </summary>
/// <param name="game"></param>
/// <param name="exception"></param>
public class InstanceSetIconErrorEventArgs(GameSettingObj game, string file, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Game => game;
    /// <summary>
    /// 图标文件
    /// </summary>
    public string File => file;
}

/// <summary>
/// Java安装失败
/// </summary>
/// <param name="name"></param>
/// <param name="url"></param>
/// <param name="exception"></param>
public class JavaInstallErrorEventArgs(string name, string url, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => name;
    /// <summary>
    /// 地址
    /// </summary>
    public string Url => url;
}

/// <summary>
/// 请求网络API时错误
/// </summary>
/// <param name="url">网址</param>
/// <param name="exception"></param>
public class ApiRequestErrorEventArgs(string url, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 请求地址
    /// </summary>
    public string Url => url;
}

/// <summary>
/// 获取游戏版本信息错误
/// </summary>
public class MojangGetVersionErrorEventArgs() : CoreErrorEventArgs(false, false)
{
}

/// <summary>
/// 玩家皮肤获取错误
/// </summary>
/// <param name="login"></param>
/// <param name="exception"></param>
public class PlayerSkinGetErrorEventArgs(LoginObj login, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 登录的账户
    /// </summary>
    public LoginObj Login => login;
}

/// <summary>
/// 玩家披风获取错误
/// </summary>
/// <param name="login"></param>
/// <param name="exception"></param>
public class PlayerCapeGetErrorEventArgs(LoginObj login, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 登录的账户
    /// </summary>
    public LoginObj Login => login;
}

/// <summary>
/// 玩家材质获取错误
/// </summary>
/// <param name="login"></param>
/// <param name="exception"></param>
public class PlayerTexturesGetErrorEventArgs(LoginObj login, Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{
    /// <summary>
    /// 登录的账户
    /// </summary>
    public LoginObj Login => login;
}

/// <summary>
/// 运行库仓库错误
/// </summary>
/// <param name="exception"></param>
public class LocalMavenErrorEventArgs(Exception exception) : ExceptionErrorEventArgs(exception, false, false)
{ 

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
    public GameLogItemObj? LogItem => log;
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