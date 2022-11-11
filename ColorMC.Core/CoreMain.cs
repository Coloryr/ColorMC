using ColorMC.Core.Config;
using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;

namespace ColorMC.Core;

public enum DownloadState
{ 
    Init, GetInfo, Start, End
}

public static class CoreMain
{
    public const string Version = "1.0.0";

    public static Action<string, Exception, bool> OnError;
    public static Action NewStart;
    public static Action DownloadUpdate;
    public static Action<DownloadState> DownloadState;
    public static Action<DownloadItem> DownloadStateUpdate;

    public static async void Init(string dir) 
    {
        SystemInfo.Init();
        Logs.Init(dir);
        ConfigUtils.Init(dir);
        VersionPath.Init(dir);
        AssetsPath.Init(dir);
        InstancesPath.Init(dir);
        LibrariesPath.Init(dir);
        DownloadManager.Init();
    }
}