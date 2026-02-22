using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Game;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core;

public static class ColorMCCore
{
    public const int VersionNum = 40;
    public const string TopVersion = "40";
    public const string DateVersion = "20260222";

    /// <summary>
    /// 版本号
    /// </summary>
    public const string Version = $"{TopVersion}.{DateVersion}";

    /// <summary>
    /// 运行路径
    /// </summary>
    public static string BaseDir { get; private set; }

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
    /// Java修改
    /// </summary>
    public static event Action<JavaChangeArg>? JavaChange;
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
    internal static CoreInitObj CoreArg;

    /// <summary>
    /// 游戏窗口句柄
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, GameHandle> s_games = [];
    /// <summary>
    /// 游戏日志
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, GameRuntimeLog> s_gameLogs = [];

    /// <summary>
    /// 初始化阶段1
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(CoreInitObj arg)
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
        ConfigSave.Init();
    }

    /// <summary>
    /// 初始化阶段2
    /// </summary>
    public static void Init1()
    {
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
    public static void KillGame(Guid uuid)
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
    internal static void AddGameHandel(Guid uuid, GameHandle handel)
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
    /// Java修改
    /// </summary>
    internal static void OnJavaChange(JavaInfoObj? java, bool add, bool mut)
    {
        JavaChange?.Invoke(new JavaChangeArg
        {
            IsAdd = add,
            IsMut = mut,
            Java = java
        });
    }

    /// <summary>
    /// 获取下载窗口句柄
    /// </summary>
    /// <returns></returns>
    internal static IDownloadGui? OnDownloadGui()
    {
        var arg = new DownloadEventArgs
        {
            Thread = ConfigLoad.Config.Http.DownloadThread
        };
        Download?.Invoke(arg);
        return arg.GuiHandle;
    }
}