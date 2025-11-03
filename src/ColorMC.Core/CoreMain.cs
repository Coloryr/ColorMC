using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core;

public static class ColorMCCore
{
    public const int VersionNum = 39;
    public const string TopVersion = "39";
    public const string DateVersion = "20251022";

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
    public delegate void NoJava(int version);
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
    /// 选择项目
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="items">项目列表</param>
    /// <returns>选择的项目</returns>
    public delegate Task<int> Select(string title, List<string> items);

    /// <summary>
    /// 显示下载窗口
    /// </summary>
    public static event Action<DownloadEventArgs>? Download;
    /// <summary>
    /// 错误显示回调
    /// </summary>
    public static event Action<CoreErrorEventArgs>? Error;
    /// <summary>
    /// 游戏日志回调
    /// </summary>
    public static event Action<GameLogEventArgs>? GameLog;
    /// <summary>
    /// 游戏退出事件
    /// </summary>
    public static event Action<GameExitEventArgs> GameExit;
    /// <summary>
    /// 游戏实例事件
    /// </summary>
    public static event Action<InstanceChangeEventArgs>? InstanceChange;
    /// <summary>
    /// 是否为新运行
    /// </summary>
    public static bool NewStart { get; internal set; }

    /// <summary>
    /// 停止事件
    /// </summary>
    internal static event Action? Stop;
    /// <summary>
    /// 启动器核心参数
    /// </summary>
    internal static CoreInitArg CoreArg;

    /// <summary>
    /// 游戏窗口句柄
    /// </summary>
    private static readonly ConcurrentDictionary<string, GameHandel> s_games = [];
    /// <summary>
    /// 游戏日志
    /// </summary>
    private static readonly ConcurrentDictionary<string, GameRuntimeLog> s_gameLogs = [];

    /// <summary>
    /// 初始化阶段1
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(CoreInitArg arg)
    {
        if (string.IsNullOrWhiteSpace(arg.Local))
        {
            throw new ArgumentNullException(nameof(arg.Local));
        }
        CoreArg = arg;

        BaseDir = arg.Local;
        Directory.CreateDirectory(BaseDir);

        ConfigLoad.Init();
        CoreHttpClient.Init();
    }

    /// <summary>
    /// 初始化阶段2
    /// </summary>
    public static void Init1()
    {
        ConfigSave.Init();
        LocalMaven.Init();
        DownloadManager.Init();
        AuthDatabase.Init();
        JvmPath.Init();
        MinecraftPath.Init(BaseDir);
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
        if (s_games.TryGetValue(uuid, out var handel))
        {
            handel.Kill();
        }
    }

    /// <summary>
    /// 启动器产生错误，并打开窗口显示
    /// </summary>
    /// <param name="args">报错信息</param>
    internal static void OnError(CoreErrorEventArgs args)
    {
        Error?.Invoke(args);
    }

    /// <summary>
    /// 运行游戏日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="text">控制台输出</param>
    internal static void OnGameLog(GameSettingObj obj, string? text)
    {
        if (s_gameLogs.TryGetValue(obj.UUID, out var log))
        {
            var item = log.AddLog(text);
            GameLog?.Invoke(new GameLogEventArgs(obj, item));
        }
    }

    /// <summary>
    /// 添加系统日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="type">系统日志类型</param>
    internal static void OnGameLog(GameSettingObj obj, GameSystemLog type)
    {
        if (s_gameLogs.TryGetValue(obj.UUID, out var log))
        {
            var item = log.AddLog(type);
            GameLog?.Invoke(new GameLogEventArgs(obj, item));
        }
    }

    /// <summary>
    /// 添加系统日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="type">系统日志类型</param>
    internal static void OnGameLog(GameSettingObj obj, GameSystemLog type, string text1, string text2)
    {
        if (s_gameLogs.TryGetValue(obj.UUID, out var log))
        {
            var item = log.AddLog(type, text1, text2);
            GameLog?.Invoke(new GameLogEventArgs(obj, item));
        }
    }

    /// <summary>
    /// 清理游戏实例日志
    /// </summary>
    /// <param name="obj"></param>
    internal static void GameLogClear(GameSettingObj obj)
    {
        if (s_gameLogs.TryGetValue(obj.UUID, out var log))
        {
            log.Clear();
        }
        else
        {
            s_gameLogs.TryAdd(obj.UUID, new());
        }
    }

    /// <summary>
    /// 游戏退出
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    /// <param name="code"></param>
    public static void OnGameExit(GameSettingObj obj, LoginObj obj1, int code)
    {
        s_games.TryRemove(obj.UUID, out _);
        GameExit?.Invoke(new GameExitEventArgs(obj, obj1, code));
    }

    /// <summary>
    /// 获取游戏实例日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>实例日志</returns>
    public static GameRuntimeLog? GetGameRuntimeLog(GameSettingObj obj)
    {
        if (s_gameLogs.TryGetValue(obj.UUID, out var log))
        {
            return log;
        }

        return null;
    }

    /// <summary>
    /// 添加游戏窗口句柄
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="handel"></param>
    internal static void AddGameHandel(string uuid, GameHandel handel)
    {
        if (!s_games.TryAdd(uuid, handel))
        {
            s_games[uuid] = handel;
        }
    }

    /// <summary>
    /// 游戏实例数量修改
    /// </summary>
    internal static void OnInstanceChange()
    {
        InstanceChange?.Invoke(new InstanceChangeEventArgs(InstanceChangeType.NumberChange));
    }

    /// <summary>
    /// 游戏图标修改
    /// </summary>
    /// <param name="obj"></param>
    internal static void OnInstanceIconChange(GameSettingObj obj)
    {
        InstanceChange?.Invoke(new InstanceChangeEventArgs(InstanceChangeType.IconChange, obj));
    }

    /// <summary>
    /// 获取下载窗口句柄
    /// </summary>
    /// <returns></returns>
    internal static DownloadEventArgs OnDownloadGui()
    {
        var arg = new DownloadEventArgs();
        Download?.Invoke(arg);
        return arg;
    }
}