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
    public const string TopVersion = "A23";
    public const string DateVersion = "20240114";

    public const string Version = $"{TopVersion}.{DateVersion}";

    /// <summary>
    /// 运行路径
    /// </summary>
    public static string BaseDir { get; private set; }

    public delegate Task<bool> GameRequest(string text);
    public delegate void ZipUpdate(string text, int size, int all);
    public delegate Task<bool> LaunchP(bool pre);
    public delegate void UpdateState(string? text);
    public delegate Task<bool> UpdateSelect(string? text);
    public delegate void NoJava();
    public delegate void JavaUnzip();
    public delegate Task<bool> LoginFail(LoginObj obj);
    public delegate void LoginOAuthCode(string url, string code);
    public delegate Task<bool> GameOverwirte(GameSettingObj obj);
    public delegate void PackUpdate(int size, int now);
    public delegate void DownloaderUpdate(CoreRunState state);
    public delegate void PackState(CoreRunState state);

    /// <summary>
    /// 错误显示回调
    /// 标题 错误 关闭程序
    /// </summary>
    public static Action<string?, Exception?, bool>? OnError { internal get; set; }
    /// <summary>
    /// 下载项目更新回调
    /// </summary>
    public static Action<DownloadItemObj>? DownloadItemUpdate { internal get; set; }
    /// <summary>
    /// 游戏启动回调
    /// </summary>
    public static Action<GameSettingObj, LaunchState>? GameLaunch { get; set; }
    /// <summary>
    /// 游戏进程日志回调
    /// </summary>
    public static Action<Process?, string?>? ProcessLog { internal get; set; }
    /// <summary>
    /// 游戏日志回调
    /// </summary>
    public static Action<GameSettingObj, string?>? GameLog { internal get; set; }
    /// <summary>
    /// 语言重载
    /// </summary>
    public static Action<LanguageType>? LanguageReload { internal get; set; }

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
    public static Func<GameSettingObj, JavaInfo, string, List<string>,
        Dictionary<string, string>, Process> PhoneJvmRun
    { internal get; set; }
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
}