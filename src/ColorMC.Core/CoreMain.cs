using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core;

public static class ColorMCCore
{
    public const string TopVersion = "A24";
    public const string DateVersion = "20240201";

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
    public delegate Task<bool> UpdateSelect(string? text);
    public delegate void NoJava();
    public delegate void JavaUnzip();
    public delegate Task<bool> LoginFail(LoginObj obj);
    public delegate void LoginOAuthCode(string url, string code);
    public delegate Task<bool> GameOverwirte(GameSettingObj obj);
    public delegate void PackUpdate(int size, int now);
    public delegate void DownloaderUpdate(DownloadState state);
    public delegate void PackState(CoreRunState state);
    public delegate void GameLaunch(GameSettingObj obj, LaunchState state);
    public delegate void DownloadItemUpdate(DownloadItemObj obj);

    /// <summary>
    /// 开始下载
    /// </summary>
    public static Func<ICollection<DownloadItemObj>, Task<bool>>? OnStartDownload;

    /// <summary>
    /// 错误显示回调
    /// 标题 错误 关闭程序
    /// </summary>
    public static event Action<string?, Exception?, bool>? OnError;
    /// <summary>
    /// 游戏进程日志回调
    /// </summary>
    public static event Action<Process?, string?>? OnProcessLog;
    /// <summary>
    /// 游戏日志回调
    /// </summary>
    public static event Action<GameSettingObj, string?>? OnGameLog;
    /// <summary>
    /// 语言重载
    /// </summary>
    public static event Action<LanguageType>? OnLanguageReload;

    /// <summary>
    /// 手机端启动
    /// </summary>
    public static Func<GameSettingObj, JavaInfo, List<string>, Dictionary<string, string>, Process>? PhoneGameLaunch { internal get; set; }
    /// <summary>
    /// 手机端Jvm安装
    /// </summary>
    public static Action<Stream, string, ZipUpdate>? PhoneJvmInstall { internal get; set; }
    /// <summary>
    /// 手机端读Java信息
    /// </summary>
    public static Func<string, Process?>? PhoneStartJvm { internal get; set; }
    /// <summary>
    /// 手机端读文件
    /// </summary>
    public static Func<string, Stream?>? PhoneReadFile { get; set; }
    /// <summary>
    /// 手机端获取运行路径
    /// </summary>
    public static Func<string>? PhoneGetDataDir { internal get; set; }
    /// <summary>
    /// 手机端Jvm运行
    /// </summary>
    public static Func<GameSettingObj, JavaInfo, string, List<string>, Dictionary<string, string>, Process> PhoneJvmRun { internal get; set; }
    /// <summary>
    /// 手机端打开网页
    /// </summary>
    public static Action<string?> PhoneOpenUrl { get; set; }

    /// <summary>
    /// 新运行
    /// </summary>
    public static bool NewStart { get; internal set; }

    /// <summary>
    /// 停止事件
    /// </summary>
    internal static event Action? Stop;

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
    public static void Init1(Action? done)
    {
        ConfigSave.Init();
        GameCount.Init(BaseDir);
        JvmPath.Init(BaseDir);
        FrpPath.Init(BaseDir);
        LocalMaven.Init(BaseDir);
        DownloadManager.Init(BaseDir);
        AuthDatabase.Init();
        MCPath.Init(BaseDir);
        done?.Invoke();

        Logs.Info(LanguageHelper.Get("Core.Info3"));
    }

    /// <summary>
    /// 执行关闭操作
    /// </summary>
    public static void Close()
    {
        Stop?.Invoke();
    }

    public static void Error(string text, Exception? e, bool close)
    {
        OnError?.Invoke(text, e, close);
        Logs.Error(text, e);
    }

    public static void GameLog(GameSettingObj obj, string? text)
    {
        OnGameLog?.Invoke(obj, text);
    }

    public static void GameLog(Process? process, string? text)
    {
        OnProcessLog?.Invoke(process, text);
    }

    public static void LanguageReload(LanguageType type)
    {
        OnLanguageReload?.Invoke(type);
    }
}