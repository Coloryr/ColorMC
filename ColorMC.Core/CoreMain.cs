using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Http;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
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

    public static Action<int> DownloadUpdate;
    public static Action<CoreRunState> DownloadState;
    public static Action<int, DownloadItem> DownloadStateUpdate;
    public static Action<int, DownloadItem, Exception> DownloadError;

    public static Func<GameSettingObj, bool> GameOverwirte;
    public static Func<GameSettingObj, bool> GameDownload;
    public static Action<GameSettingObj, LaunchState> GameLaunch;

    public static Action<CoreRunState> PackState;
    public static Action<int, int> PackUpdate;

    public static Action<Process?, string?> ProcessLog;

    public static Action<AuthState> AuthStateUpdate;
    public static Action<string, string> LoginOAuthCode;
    public static Action<LoginState, string> LoginFail;

    public static void Init(string dir)
    {
        Logs.Init(dir);
        ConfigUtils.Init(dir);
        SystemInfo.Init();
        BaseClient.Init();
        DownloadManager.Init();
        AuthDatabase.Init();
        MCPath.Init(dir);

        Logs.Info("ColorMC核心已初始化");
    }
}