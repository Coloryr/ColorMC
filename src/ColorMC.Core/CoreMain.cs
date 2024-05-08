using System.Collections.Concurrent;
using System.Diagnostics;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace ColorMC.Core;

public static class ColorMCCore
{
    public const string TopVersion = "A25";
    public const string DateVersion = "20240508";

    /// <summary>
    /// 版本号
    /// </summary>
    public const string Version = $"{TopVersion}.{DateVersion}";

    /// <summary>
    /// 运行路径
    /// </summary>
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 请求回调
    /// </summary>
    /// <param name="text">显示内容</param>
    /// <returns>是否同意</returns>
    public delegate Task<bool> Request(string text);
    /// <summary>
    /// 压缩包更新
    /// </summary>
    /// <param name="text">名字</param>
    /// <param name="size">目前进度</param>
    /// <param name="all">总进度</param>
    public delegate void ZipUpdate(string text, int size, int all);
    /// <summary>
    /// 请求是否运行程序
    /// </summary>
    /// <param name="pre">是否为运行前启动</param>
    /// <returns>是否同意</returns>
    public delegate Task<bool> LaunchP(bool pre);
    /// <summary>
    /// 状态发生改变
    /// </summary>
    /// <param name="text">消息</param>
    public delegate void UpdateState(string? text);
    /// <summary>
    /// 启动选择框
    /// </summary>
    /// <param name="text">消息</param>
    /// <returns>是否确定</returns>
    public delegate Task<bool> ChoiseCall(string? text);
    /// <summary>
    /// 没有Java
    /// </summary>
    public delegate void NoJava();
    /// <summary>
    /// Java解压
    /// </summary>
    public delegate void JavaUnzip();
    /// <summary>
    /// 登录失败是否继续运行
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns>是否继续运行</returns>
    public delegate Task<bool> LoginFailRun(LoginObj obj);
    /// <summary>
    /// OAuth登录
    /// </summary>
    /// <param name="url">网址</param>
    /// <param name="code">登陆码</param>
    public delegate void LoginOAuthCode(string url, string code);
    /// <summary>
    /// 游戏复写
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>是否复写</returns>
    public delegate Task<bool> GameOverwirte(GameSettingObj obj);
    /// <summary>
    /// 整合包进度更新
    /// </summary>
    /// <param name="size">总进度</param>
    /// <param name="now">目前进度</param>
    public delegate void PackUpdate(int size, int now);
    /// <summary>
    /// 下载器状态更新
    /// </summary>
    /// <param name="state">状态</param>
    public delegate void DownloaderUpdate(DownloadState state);
    /// <summary>
    /// 下载项目状态更新
    /// </summary>
    /// <param name="obj">项目</param>
    public delegate void DownloadItemUpdate(DownloadItemObj obj);
    /// <summary>
    /// 压缩包导入状态改变
    /// </summary>
    /// <param name="state">状态</param>
    public delegate void PackState(CoreRunState state);
    /// <summary>
    /// 游戏启动信息更新
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="state">当前状态</param>
    public delegate void GameLaunch(GameSettingObj obj, LaunchState state);

    /// <summary>
    /// 下载用的回调
    /// </summary>
    public static Func<ICollection<DownloadItemObj>, Task<bool>>? OnDownload { get; set; }

    /// <summary>
    /// 错误显示回调
    /// 标题 错误 关闭程序
    /// </summary>
    public static event Action<string?, Exception?, bool>? Error;
    /// <summary>
    /// 游戏日志回调
    /// </summary>
    public static event Action<GameSettingObj, string?>? GameLog;
    /// <summary>
    /// 语言重载
    /// </summary>
    public static event Action<LanguageType>? LanguageReload;
    /// <summary>
    /// 收到游戏数据包
    /// </summary>
    public static event Action<IChannel, IByteBuffer>? NettyPack;
    /// <summary>
    /// 游戏退出事件
    /// </summary>
    public static event Action<GameSettingObj, LoginObj, int> GameExit;

    /// <summary>
    /// 手机端启动
    /// </summary>
    public static Func<GameSettingObj, JavaInfo, List<string>, Dictionary<string, string>, Process> PhoneGameLaunch { internal get; set; }
    /// <summary>
    /// 手机端Jvm安装
    /// </summary>
    public static Action<Stream, string, ZipUpdate> PhoneJvmInstall { internal get; set; }
    /// <summary>
    /// 手机端读Java信息
    /// </summary>
    public static Func<string, Process?> PhoneStartJvm { internal get; set; }
    /// <summary>
    /// 手机端读文件
    /// </summary>
    public static Func<string, Stream?> PhoneReadFile { get; set; }
    /// <summary>
    /// 手机端获取运行路径
    /// </summary>
    public static Func<string> PhoneGetDataDir { internal get; set; }
    /// <summary>
    /// 手机端Jvm运行
    /// </summary>
    public static Func<GameSettingObj, JavaInfo, string, List<string>, Dictionary<string, string>, Process> PhoneJvmRun { internal get; set; }
    /// <summary>
    /// 手机端打开网页
    /// </summary>
    public static Action<string?> PhoneOpenUrl { get; set; }
    /// <summary>
    /// 获取一个空闲端口
    /// </summary>
    public static Func<int> GetFreePort { get; set; }

    /// <summary>
    /// 是否为新运行
    /// </summary>
    public static bool NewStart { get; internal set; }

    /// <summary>
    /// 停止事件
    /// </summary>
    internal static event Action? Stop;

    /// <summary>
    /// 游戏窗口句柄
    /// </summary>
    internal static ConcurrentDictionary<string, IGameHandel> Games = [];

    /// <summary>
    /// 初始化阶段1
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir;
        Directory.CreateDirectory(dir);
        LanguageHelper.Load(LanguageType.zh_cn);
        Logs.Init(dir);
        ToolPath.Init(dir);
        ConfigUtils.Init(dir);
        BaseClient.Init();

        Logs.Info(LanguageHelper.Get("Core.Info1"));
    }

    /// <summary>
    /// 初始化阶段2
    /// </summary>
    public static void Init1()
    {
        ConfigSave.Init();
        GameCount.Init(BaseDir);
        JvmPath.Init(BaseDir);
        FrpPath.Init(BaseDir);
        LocalMaven.Init(BaseDir);
        DownloadManager.Init(BaseDir);
        AuthDatabase.Init();
        MCPath.Init(BaseDir);

        Logs.Info(LanguageHelper.Get("Core.Info3"));
    }

    /// <summary>
    /// 执行关闭操作
    /// </summary>
    public static void Close()
    {
        Stop?.Invoke();
    }

    /// <summary>
    /// 强制关闭游戏
    /// </summary>
    /// <param name="uuid"></param>
    public static void KillGame(string uuid)
    {
        if (Games.TryGetValue(uuid, out var handel))
        {
            handel.Kill();
        }
    }

    /// <summary>
    /// 启动器产生错误
    /// </summary>
    /// <param name="text"></param>
    /// <param name="e"></param>
    /// <param name="close"></param>
    internal static void OnError(string text, Exception? e, bool close)
    {
        Error?.Invoke(text, e, close);
        Logs.Error(text, e);
    }

    /// <summary>
    /// 游戏日志
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="text"></param>
    internal static void OnGameLog(GameSettingObj obj, string? text)
    {
        GameLog?.Invoke(obj, text);
    }

    /// <summary>
    /// 语言重载
    /// </summary>
    /// <param name="type"></param>
    internal static void OnLanguageReload(LanguageType type)
    {
        LanguageReload?.Invoke(type);
    }

    /// <summary>
    /// 游戏推出
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    /// <param name="code"></param>
    internal static void OnGameExit(GameSettingObj obj, LoginObj obj1, int code)
    {
        Games.TryRemove(obj.UUID, out _);
        GameExit?.Invoke(obj, obj1, code);
    }

    /// <summary>
    /// 添加游戏窗口句柄
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="handel"></param>
    internal static void AddGameHandel(string uuid, IGameHandel handel)
    {
        if (!Games.TryAdd(uuid, handel))
        {
            Games[uuid] = handel;
        }
    }

    /// <summary>
    /// 游戏发送消息给启动器
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="buffer"></param>
    internal static void OnNettyPack(IChannel channel, IByteBuffer buffer)
    {
        NettyPack?.Invoke(channel, buffer);
    }
}