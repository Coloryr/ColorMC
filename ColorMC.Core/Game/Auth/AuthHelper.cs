using ColorMC.Core.Http;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.Game;
using Newtonsoft.Json;

namespace ColorMC.Core.Game.Auth;

public static class AuthHelper
{
    public static string NowAuthlibInjector { get; private set; }
    public static DownloadItem BuildNide8Item()
    {
        return new()
        {
            Url = "https://login.mc-user.com:233/download/nide8auth.jar",
            Name = "com.nide8.login2:nide8auth:2.3",
            Local = $"{LibrariesPath.BaseDir}/com/nide8/login2/nide8auth/2.3/nide8auth.2.3.jar",
        };
    }

    public static DownloadItem BuildAuthlibInjectorItem(AuthlibInjectorObj obj)
    {
        return new()
        {
            SHA256 = obj.checksums.sha256,
            Url = obj.download_url,
            Name = $"moe.yushi:authlibinjector:{obj.version}",
            Local = $"{LibrariesPath.BaseDir}/moe/yushi/authlibinjector/{obj.version}/authlib-injector-{obj.version}.jar",
        };
    }

    public static DownloadItem ReadyNide8()
    {
        var item = BuildNide8Item();
        if(!File.Exists(item.Local))
            return item;

        return null;
    }

    public static async Task<DownloadItem?> ReadyAuthlibInjector()
    {
        var meta = await BaseClient.GetString(UrlHelp.AuthlibInjectorMeta(BaseClient.Source));
        var obj = JsonConvert.DeserializeObject<AuthlibInjectorMetaObj>(meta);
        var item = obj.artifacts.Where(a => a.build_number == obj.latest_build_number).First();

        var info = await BaseClient.GetString(UrlHelp.AuthlibInjector(item, BaseClient.Source));
        var obj1 = JsonConvert.DeserializeObject<AuthlibInjectorObj>(meta);

        var item1 = BuildAuthlibInjectorItem(obj1);
        
        NowAuthlibInjector = item1.Local;
        if (!File.Exists(NowAuthlibInjector))
            return item1;
        else
            return null;
    }
}
