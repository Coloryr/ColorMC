using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 外置登录器
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

    private static AuthlibInjectorObj LocalAuthLib = new()
    {
        build_number = 51,
        checksums = new() { sha256 = "d3ec36486b0a5ab5a16069733cd8d749950c2d62e1bdacaf27864b30b92c1c7f" },
        download_url = "https://authlib-injector.yushi.moe/artifact/51/authlib-injector-1.2.3.jar",
        version = "1.2.3"
    };

    /// <summary>
    /// 创建Nide8Injector下载实例
    /// </summary>
    /// <returns>下载实例</returns>
    private static DownloadItemObj BuildNide8Item(string version)
    {
        return new()
        {
            Url = UrlHelper.Nide8Jar,
            Name = $"com.nide8.login2:nide8auth:{version}",
            Local = $"{LibrariesPath.BaseDir}/com/nide8/login2/nide8auth/{version}/nide8auth-{version}.jar",
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
    public static async Task<(bool, DownloadItemObj?)> ReadyNide8()
    {
        var data = await BaseClient.GetStringAsync($"{UrlHelper.Nide8}00000000000000000000000000000000/");
        if (data.Item1 == false)
        {
            return (false, null);
        }
        try
        {
            var obj = JObject.Parse(data.Item2!);
            var sha1 = obj?["jarHash"]!.ToString().ToLower();
            var item = BuildNide8Item(obj!["jarVersion"]!.ToString());
            NowNide8Injector = item.Local;

            item.SHA1 = sha1;
            if (!File.Exists(NowNide8Injector))
            {
                return (true, item);
            }

            if (!string.IsNullOrWhiteSpace(sha1))
            {
                using var stream = PathHelper.OpenRead(NowNide8Injector)!;
                var sha11 = HashHelper.GenSha1(stream);
                if (sha11 != sha1)
                {
                    return (true, item);
                }
            }

            return (true, null);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error8"), e);
            return (false, null);
        }
    }

    /// <summary>
    /// 获取AuthlibInjector信息
    /// </summary>
    /// <returns>信息</returns>
    /// <exception cref="Exception">获取失败</exception>
    private static async Task<AuthlibInjectorObj> GetAuthlibInjectorObjAsync()
    {
        try
        {
            string url = UrlHelper.AuthlibInjectorMeta(BaseClient.Source);
            var meta = await BaseClient.GetStringAsync(url);
            if (meta.Item1 == false)
            {
                return LocalAuthLib;
            }
            var obj = JsonConvert.DeserializeObject<AuthlibInjectorMetaObj>(meta.Item2!);
            if (obj == null)
            {
                return LocalAuthLib;
            }
            var item = obj.artifacts.Where(a => a.build_number == obj.latest_build_number).ToList()[0];

            var info = await BaseClient.GetStringAsync(UrlHelper.AuthlibInjector(item, BaseClient.Source));
            if (info.Item1 == false)
            {
                return LocalAuthLib;
            }
            return JsonConvert.DeserializeObject<AuthlibInjectorObj>(info.Item2!) ?? LocalAuthLib;
        }
        catch
        {
            return LocalAuthLib;
        }
    }

    /// <summary>
    /// 初始化AuthlibInjector，存在不下载
    /// </summary>
    /// <returns>AuthlibInjector下载实例</returns>
    public static async Task<(bool, DownloadItemObj?)> ReadyAuthlibInjectorAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NowAuthlibInjector))
            {
                var obj1 = await GetAuthlibInjectorObjAsync();
                var item1 = BuildAuthlibInjectorItem(obj1);

                LocalMaven.AddItem(new()
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
                        using var stream = PathHelper.OpenRead(NowAuthlibInjector)!;
                        var sha2561 = await HashHelper.GenSha256Async(stream);
                        if (sha256 != sha2561)
                        {
                            return (true, item1);
                        }
                    }
                }
                else
                {
                    return (true, item1);
                }

                return (true, null);
            }
            else if (File.Exists(NowAuthlibInjector))
            {
                var item = LocalMaven.GetItem("moe.yushi:authlibinjector");
                if (item != null && !string.IsNullOrWhiteSpace(item.SHA256))
                {
                    using var stream = PathHelper.OpenRead(NowAuthlibInjector)!;
                    var sha2561 = await HashHelper.GenSha256Async(stream);
                    if (item.SHA256 != sha2561)
                    {
                        var obj1 = await GetAuthlibInjectorObjAsync();
                        return (true, BuildAuthlibInjectorItem(obj1));
                    }
                }
            }

            return (true, null);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error11"), e);
            return (false, null);
        }
    }
}
