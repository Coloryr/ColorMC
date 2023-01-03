using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core;

public enum CoreRunState
{
    Read, Init, GetInfo, Start, End,
    Download,
    Error,
}

public static class CoreMain
{
    public const string Version = "1.0.0";

    /// <summary>
    /// 错误显示回调
    /// 标题 错误 关闭程序
    /// </summary>
    public static Action<string, Exception, bool>? OnError;
    public static Action? NewStart;

    /// <summary>
    /// 下载线程相应回调
    /// </summary>
    public static Action<CoreRunState>? DownloaderUpdate { get; set; }
    public static Action<int, DownloadItem>? DownloadItemStateUpdate { get; set; }
    public static Action<int, DownloadItem, Exception>? DownloadItemError { get; set; }

    public static Func<GameSettingObj, Task<bool>>? GameOverwirte { get; set; }
    public static Func<LaunchState, GameSettingObj, Task<bool>>? GameDownload { get; set; }
    public static Action<GameSettingObj, LaunchState>? GameLaunch { get; set; }

    public static Action<CoreRunState>? PackState { get; set; }
    public static Action<int, int>? PackUpdate { get; set; }

    public static Action<Process?, string?>? ProcessLog { get; set; }

    public static Action<AuthState>? AuthStateUpdate { get; set; }
    public static Action<string, string>? LoginOAuthCode { get; set; }

    public static void Init(string dir)
    {
        Logs.Init(dir);
        ConfigUtils.Init(dir);
        SystemInfo.Init();
        DownloadManager.Init();
        AuthDatabase.Init();
        MCPath.Init(dir);

        Logs.Info("ColorMC核心已初始化");
    }
}