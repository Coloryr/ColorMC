using ColorMC.Core.Http.Downloader;
using ColorMC.Core.LaunchPath;

namespace ColorMC.Core.Game.Auth;

public static class AuthHelper
{
    public static DownloadItem BuildNide8Item()
    {
        return new()
        {
            Url = "https://login.mc-user.com:233/download/nide8auth.jar",
            Name = "com.nide8.login2:nide8auth:2.3",
            Local = $"{LibrariesPath.BaseDir}/com/nide8/login2/nide8auth/2.3/nide8auth.2.3.jar",
        };
    }

    public static async Task<string> ReadyNide8()
    {
        var item = BuildNide8Item();
        await DownloadThread.Download(item);

        return item.Local;
    }
}
