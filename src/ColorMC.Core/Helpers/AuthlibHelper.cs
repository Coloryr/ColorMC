using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 外置登陆器
/// </summary>
public static class AuthlibHelper
{
    /// <summary>
    /// AuthlibInjector 物理位置
    /// </summary>
    public static string NowAuthlibInjector { get; private set; }
    /// <summary>
    /// Nide8Injector 物理位置
    /// </summary>
    public static string NowNide8Injector { get; private set; }

    /// <summary>
    /// 创建Nide8Injector下载实例
    /// </summary>
    /// <returns>下载实例</returns>
    private static DownloadItemObj BuildNide8Item()
    {
        return new()
        {
            Url = UrlHelper.Nide8Jar,
            Name = "com.nide8.login2:nide8auth:2.4",
            Local = $"{LibrariesPath.BaseDir}/com/nide8/login2/nide8auth/2.4/nide8auth.2.4.jar",
        };
    }
    /// <summary>
    /// 创建AuthlibInjector下载实例
    /// </summary>
    /// <param name="obj">AuthlibInjector信息</param>
    /// <returns>下载实例</returns>
    private static DownloadItemObj BuildAuthlibInjectorItem(AuthlibInjectorObj obj)
    {
        return new()
        {
            SHA256 = obj.checksums.sha256,
            Url = UrlHelper.DownloadAuthlibInjector(obj, BaseClient.Source),
            Name = $"moe.yushi:authlibinjector:{obj.version}",
            Local = $"{LibrariesPath.BaseDir}/moe/yushi/authlibinjector/" +
            $"{obj.version}/authlib-injector-{obj.version}.jar",
        };
    }

    /// <summary>
    /// 初始化Nide8Injector，存在不下载
    /// </summary>
    /// <returns>Nide8Injector下载实例</returns>
    public static async Task<DownloadItemObj?> ReadyNide8()
    {
        var item = BuildNide8Item();
        NowNide8Injector = item.Local;

        var data = await BaseClient.GetString($"{UrlHelper.Nide8}00000000000000000000000000000000/");
        if (data.Item1 == false)
            return null;
        try
        {
            var obj = JObject.Parse(data.Item2!);
            var sha1 = obj["jarHash"]!.ToString().ToLower();
            item.SHA1 = sha1;
            if (!File.Exists(NowNide8Injector))
            {
                return item;
            }

            if (!string.IsNullOrWhiteSpace(sha1))
            {
                using var stream = File.OpenRead(NowNide8Injector);
                var sha11 = Funtions.GenSha1(stream);
                if (sha11 != sha1)
                {
                    return item;
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error8"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取AuthlibInjector信息
    /// </summary>
    /// <returns>信息</returns>
    /// <exception cref="Exception">获取失败</exception>
    private static async Task<AuthlibInjectorObj> GetAuthlibInjectorObj()
    {
        string url = UrlHelper.AuthlibInjectorMeta(BaseClient.Source);
        var meta = await BaseClient.GetString(url);
        if (meta.Item1 == false)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                new Exception(url), false);
            throw new Exception(LanguageHelper.Get("AuthlibInjector.Error1"));
        }
        var obj = JsonConvert.DeserializeObject<AuthlibInjectorMetaObj>(meta.Item2!)
            ?? throw new Exception(LanguageHelper.Get("AuthlibInjector.Error1"));
        var item = obj.artifacts.Where(a => a.build_number == obj.latest_build_number).First();

        var info = await BaseClient.GetString(UrlHelper.AuthlibInjector(item, BaseClient.Source));
        if (info.Item1 == false)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"), new Exception(url), false);
            throw new Exception(LanguageHelper.Get("AuthlibInjector.Error1"));
        }
        return JsonConvert.DeserializeObject<AuthlibInjectorObj>(info.Item2!)
            ?? throw new Exception(LanguageHelper.Get("AuthlibInjector.Error1"));
    }

    /// <summary>
    /// 初始化AuthlibInjector，存在不下载
    /// </summary>
    /// <returns>AuthlibInjector下载实例</returns>
    public static async Task<DownloadItemObj?> ReadyAuthlibInjector()
    {
        if (string.IsNullOrWhiteSpace(NowAuthlibInjector))
        {
            var obj1 = await GetAuthlibInjectorObj();
            var item1 = BuildAuthlibInjectorItem(obj1);

            LocalMaven.AddItem(new MavenItemObj()
            {
                Name = $"moe.yushi:authlibinjector",
                Url = item1.Url,
                Have = true,
                Local = "/moe/yushi/authlibinjector/" +
                $"{obj1.version}/authlib-injector-{obj1.version}.jar",
                SHA256 = obj1.checksums.sha256
            });

            NowAuthlibInjector = item1.Local;
            if (File.Exists(NowAuthlibInjector))
            {
                var sha256 = obj1.checksums?.sha256;
                if (!string.IsNullOrWhiteSpace(sha256))
                {
                    using var stream = File.OpenRead(NowAuthlibInjector);
                    var sha2561 = Funtions.GenSha256(stream);
                    if (sha256 != sha2561)
                    {
                        return item1;
                    }
                }
            }
            else
            {
                return item1;
            }

            return null;
        }
        else if (File.Exists(NowAuthlibInjector))
        {
            var item = LocalMaven.GetItem("moe.yushi:authlibinjector");
            if (item != null && !string.IsNullOrWhiteSpace(item.SHA256))
            {
                using var stream = File.OpenRead(NowAuthlibInjector);
                var sha2561 = Funtions.GenSha256(stream);
                if (item.SHA256 != sha2561)
                {
                    var obj1 = await GetAuthlibInjectorObj();
                    return BuildAuthlibInjectorItem(obj1);
                }
            }
        }

        return null;
    }
}
