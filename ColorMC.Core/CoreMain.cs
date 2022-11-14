using ColorMC.Core.Game;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Login;
using ColorMC.Core.Objs;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core;

public enum CoreRunState
{
    Init, GetInfo, Start, End,
    Error,
}

public static class CoreMain
{
    public const string Version = "1.0.0";

    /// <summary>
    /// 错误显示回调
    /// 标题 错误 关闭程序
    /// </summary>
    public static Action<string, Exception, bool> OnError;
    public static Action NewStart;

    public static Action DownloadUpdate;
    public static Action<CoreRunState> DownloadState;
    public static Action<DownloadItem> DownloadStateUpdate;
    public static Action<DownloadItem, Exception> DownloadError;

    public static Func<GameSettingObj, bool> GameOverwirte;
    public static Func<GameSettingObj, bool> GameDownload;
    public static Action<GameSettingObj, LaunchState> GameLaunch;

    public static Action<CoreRunState> PackState;
    public static Action<int, int> PackUpdate;

    public static Action<AuthState> AuthState;

    public static Action<Process?, string?> ProcessLog;

    public static void Init(string dir)
    {
        SystemInfo.Init();
        Logs.Init(dir);
        ConfigUtils.Init(dir);
        MCPath.Init(dir);
        DownloadManager.Init();
    }
}