using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using Newtonsoft.Json;

namespace ColorMC.Core.Utils;

/// <summary>
/// 本地缓存
/// </summary>
public static class LocalMaven
{
    public const string Name1 = "maven.json";

    private static readonly ConcurrentDictionary<string, MavenItemObj> s_items = [];
    private static string s_local;
    /// <summary>
    /// 初始化本地缓存
    /// </summary>
    /// <param name="dir"></param>
    public static void Init(string dir)
    {
        s_local = dir + Name1;

        if (File.Exists(s_local))
        {
            try
            {
                var data = PathHelper.ReadText(s_local)!;
                var list = JsonConvert.DeserializeObject<Dictionary<string, MavenItemObj>>(data);

                if (list != null)
                {
                    s_items.Clear();

                    foreach (var item in list)
                    {
                        s_items.TryAdd(item.Key, item.Value);
                    }
                }
            }
            catch
            {

            }
        }
    }

    /// <summary>
    /// 获取一个本地缓存
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static MavenItemObj? GetItem(string name)
    {
        if (s_items.TryGetValue(name, out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 添加一个本地缓存
    /// </summary>
    /// <param name="item"></param>
    public static void AddItem(MavenItemObj item)
    {
        if (s_items.ContainsKey(item.Name))
        {
            s_items[item.Name] = item;
        }
        else
        {
            s_items.TryAdd(item.Name, item);
        }

        ConfigSave.AddItem(new()
        {
            Name = Name1,
            Local = s_local,
            Obj = s_items
        });
    }

    /// <summary>
    /// 创建本地缓存
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static async Task<DownloadItemObj?> MakeItemAsync(string name, string dir)
    {
        var item2 = GetItem(name);
        if (item2 != null)
        {
            if (item2.Have)
            {
                return new()
                {
                    Name = name,
                    Url = item2.Url,
                    Local = $"{LibrariesPath.BaseDir}/{item2.Local}",
                    SHA1 = item2.SHA1
                };
            }
        }
        else
        {
            try
            {
                DownloadItemObj? item3 = null;
                var maven = new MavenItemObj()
                {
                    Name = name
                };
                var url = (BaseClient.Source == SourceLocal.Offical ?
                    UrlHelper.MavenUrl[0] :
                    UrlHelper.MavenUrl[1]) + dir;
                var res = await BaseClient.DownloadClient
                    .GetAsync(url + ".sha1",
                    HttpCompletionOption.ResponseHeadersRead);
                if (res.IsSuccessStatusCode)
                {
                    item3 = new DownloadItemObj()
                    {
                        Name = name,
                        Url = url,
                        Local = $"{LibrariesPath.BaseDir}/{dir}",
                        SHA1 = await res.Content.ReadAsStringAsync()
                    };

                    maven.Have = true;
                    maven.SHA1 = item3.SHA1;
                    maven.Url = item3.Url;
                    maven.Local = dir;
                }
                else
                {
                    maven.Have = false;
                }

                AddItem(maven);

                return item3;
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Maven.Error1"), e);
            }
        }

        return null;
    }
}
