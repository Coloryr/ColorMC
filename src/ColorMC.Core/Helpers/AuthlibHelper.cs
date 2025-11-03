using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

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

    /// <summary>
    /// 现有的authlib储存
    /// </summary>
    private static readonly AuthlibInjectorObj LocalAuthLib = new()
    {
        BuildNumber = 54,
        Checksums = new() { Sha256 = "6dff7951c41d61eba78f9b034be610ade0e80d3309300742057d1866375d0427" },
        DownloadUrl = "https://authlib-injector.yushi.moe/artifact/54/authlib-injector-1.2.6.jar",
        Version = "1.2.6"
    };

    /// <summary>
    /// 创建Nide8Injector下载实例
    /// </summary>
    /// <param name="version">版本</param>
    /// <returns>下载实例</returns>
    private static FileItemObj BuildNide8Item(string version)
    {
        return new()
        {
            Url = UrlHelper.Nide8Jar,
            Name = $"com.nide8.login2:nide8auth:{version}",
            Local = Path.Combine(LibrariesPath.BaseDir, "com", "nide8", "login2", "nide8auth", version, $"nide8auth-{version}.jar"),
        };
    }
    /// <summary>
    /// 创建AuthlibInjector下载实例
    /// </summary>
    /// <param name="obj">AuthlibInjector信息</param>
    /// <returns>下载实例</returns>
    private static FileItemObj BuildAuthlibInjectorItem(AuthlibInjectorObj obj)
    {
        return new()
        {
            Sha256 = obj.Checksums.Sha256,
            Url = UrlHelper.DownloadAuthlibInjector(obj, CoreHttpClient.Source),
            Name = $"moe.yushi:authlibinjector:{obj.Version}",
            Local = Path.Combine(LibrariesPath.BaseDir, "moe", "yushi", "authlibinjector", obj.Version, $"authlib-injector-{obj.Version}.jar"),
        };
    }

    /// <summary>
    /// 初始化Nide8Injector，存在不下载
    /// </summary>
    /// <returns>Nide8Injector下载实例</returns>
    public static async Task<FileItemObj?> ReadyNide8Async(CancellationToken token)
    {
        var data = await CoreHttpClient.GetStringAsync($"{UrlHelper.Nide8}00000000000000000000000000000000/", token);
        if (data.State == false)
        {
            throw new LaunchException(LaunchError.LoginCoreError);
        }

        var obj = JsonUtils.ReadObj(data.Data!)!;
        var sha1 = obj.GetString("jarHash")!;
        var item = BuildNide8Item(obj.GetString("jarVersion")!);
        NowNide8Injector = item.Local;

        item.Sha1 = sha1;
        if (!File.Exists(NowNide8Injector))
        {
            return item;
        }

        if (!string.IsNullOrWhiteSpace(sha1))
        {
            using var stream = PathHelper.OpenRead(NowNide8Injector)!;
            var sha11 = HashHelper.GenSha1(stream);
            if (sha11 != sha1)
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取AuthlibInjector信息
    /// </summary>
    /// <returns>信息</returns>
    private static async Task<AuthlibInjectorObj> GetAuthlibInjectorObjAsync(CancellationToken token)
    {
        try
        {
            string url = UrlHelper.AuthlibInjectorMeta(CoreHttpClient.Source);
            var meta = await CoreHttpClient.GetStringAsync(url, token);
            if (meta.State == false)
            {
                return LocalAuthLib;
            }
            var obj = JsonUtils.ToObj(meta.Data!, JsonType.AuthlibInjectorMetaObj);
            if (obj == null)
            {
                return LocalAuthLib;
            }
            var item = obj.Artifacts.Where(a => a.BuildNumber == obj.LatestBuildNumber).ToList()[0];

            var info = await CoreHttpClient.GetStringAsync(UrlHelper.AuthlibInjector(item, CoreHttpClient.Source), token);
            if (info.State == false)
            {
                return LocalAuthLib;
            }
            return JsonUtils.ToObj(info.Data!, JsonType.AuthlibInjectorObj) ?? LocalAuthLib;
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
    public static async Task<MakeDownloadItemRes> ReadyAuthlibInjectorAsync(CancellationToken token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NowAuthlibInjector))
            {
                var obj1 = await GetAuthlibInjectorObjAsync(token);
                var item1 = BuildAuthlibInjectorItem(obj1);

                LocalMaven.AddItem(new()
                {
                    Name = "moe.yushi:authlibinjector",
                    Url = item1.Url,
                    Have = true,
                    Local = $"/moe/yushi/authlibinjector/{obj1.Version}/authlib-injector-{obj1.Version}.jar",
                    SHA256 = obj1.Checksums.Sha256
                });

                NowAuthlibInjector = item1.Local;
                if (File.Exists(NowAuthlibInjector))
                {
                    var sha256 = obj1.Checksums?.Sha256;
                    if (!string.IsNullOrWhiteSpace(sha256))
                    {
                        using var stream = PathHelper.OpenRead(NowAuthlibInjector)!;
                        var sha2561 = await HashHelper.GenSha256Async(stream);
                        if (sha256 != sha2561)
                        {
                            return new MakeDownloadItemRes
                            {
                                State = true,
                                Item = item1
                            };
                        }
                    }
                }
                else
                {
                    return new MakeDownloadItemRes
                    {
                        State = true,
                        Item = item1
                    };
                }

                return new MakeDownloadItemRes
                {
                    State = true
                };
            }
            else if (File.Exists(NowAuthlibInjector))
            {
                var item = LocalMaven.GetItem("moe.yushi:authlibinjector");
                if (item != null && !string.IsNullOrWhiteSpace(item.SHA256))
                {
                    using var stream = PathHelper.OpenRead(NowAuthlibInjector)!;
                    var sha256 = await HashHelper.GenSha256Async(stream);
                    if (item.SHA256 != sha256)
                    {
                        var obj1 = await GetAuthlibInjectorObjAsync(token);
                        return new MakeDownloadItemRes
                        {
                            State = true,
                            Item = BuildAuthlibInjectorItem(obj1)
                        };
                    }
                }
            }

            return new MakeDownloadItemRes
            {
                State = true
            };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Error13"), e);
            return new MakeDownloadItemRes
            {
                State = false
            };
        }
    }
}
