using System.Net;
using System.Xml;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Forge网络请求
/// </summary>
public static class ForgeAPI
{
    //支持的版本与加载器版本
    private static List<string>? s_supportVersion;
    private static readonly Dictionary<string, List<string>> s_forgeVersion = [];

    private static List<string>? s_neoSupportVersion;
    private static readonly Dictionary<string, List<string>> s_neoForgeVersion = [];

    private static async Task<Stream?> SendAsync(string url)
    {
        var data = await CoreHttpClient.GetAsync(url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Http.Error7"),
                new Exception(url), false);
            return null;
        }

        return await data.Content.ReadAsStreamAsync();
    }

    /// <summary>
    /// 获取支持的版本
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>?> GetSupportVersion(bool neo, SourceLocal? local = null)
    {
        try
        {
            if ((neo ? s_neoSupportVersion : s_supportVersion) != null)
                return neo ? s_neoSupportVersion : s_supportVersion;

            if (local == SourceLocal.BMCLAPI)
            {
                if (neo)
                {
                    if (s_neoSupportVersion == null)
                    {
                        await LoadFromSource("", true);
                    }
                    return s_neoSupportVersion;
                }
                string url = UrlHelper.ForgeVersion(local);
                using var stream = await SendAsync(url);
                var obj = JsonUtils.ToObj(stream, JsonType.ListString);
                if (obj == null)
                {
                    return null;
                }

                StringHelper.VersionSort(obj);

                if (neo)
                    s_neoSupportVersion = obj;
                else
                    s_supportVersion = obj;

                return obj;
            }
            else
            {
                await LoadFromSource("", neo);

                return neo ? s_neoSupportVersion : s_supportVersion;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error5"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取版本列表
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="source">下载源</param>
    /// <returns>版本列表</returns>
    public static async Task<List<string>?> GetVersionList(bool neo, string mc, SourceLocal? source = null)
    {
        try
        {
            bool v222 = CheckHelpers.IsGameVersion1202(mc);
            List<string> list = [];
            if (source == SourceLocal.BMCLAPI)
            {
                string url = neo
                    ? UrlHelper.NeoForgeVersions(mc, v222, source)
                    : UrlHelper.ForgeVersions(mc, source);
                using var stream = await SendAsync(url);
                
                var list1 = new List<string>();
                if (neo)
                {
                    var obj = JsonUtils.ToObj(stream, JsonType.ListNeoForgeVersionBmclApiObj);
                    if (obj == null)
                        return null;

                    foreach (var item in obj)
                    {
                        list1.Add(item.Version.Replace("neoforge-", ""));
                    }
                }
                else
                {
                    var obj = JsonUtils.ToObj(stream, JsonType.ListForgeVersionBmclApiObj);
                    if (obj == null)
                        return null;

                    foreach (var item in obj)
                    {
                        list1.Add(item.Version);
                    }
                }

                StringHelper.VersionSort(list1);

                return list1;
            }
            else
            {
                if (!neo && s_forgeVersion.TryGetValue(mc, out var list1))
                {
                    var list2 = new List<string>();
                    foreach (var item in list1)
                    {
                        list2.Add(item.ToString());
                    }
                    return list2;
                }
                else if (neo && s_neoForgeVersion.TryGetValue(mc, out list1))
                {
                    var list2 = new List<string>();
                    foreach (var item in list1)
                    {
                        list2.Add(item.ToString());
                    }
                    return list2;
                }

                await LoadFromSource(mc, neo);

                if (neo)
                {
                    if (s_neoForgeVersion.TryGetValue(mc, out var list4))
                    {
                        var list2 = new List<string>();
                        foreach (var item in list4)
                        {
                            list2.Add(item.ToString());
                        }
                        return list2;
                    }
                }
                else
                {
                    if (s_forgeVersion.TryGetValue(mc, out var list4))
                    {
                        var list2 = new List<string>();
                        foreach (var item in list4)
                        {
                            list2.Add(item.ToString());
                        }
                        return list2;
                    }
                }
                return null;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error6"), e);
            return null;
        }
    }

    /// <summary>
    /// 从xml获取信息
    /// </summary>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns></returns>
    public static async Task LoadFromSource(string mc, bool neo)
    {
        var url = neo ? UrlHelper.NeoForgeVersions(mc, false, SourceLocal.Offical) :
                    UrlHelper.ForgeVersion(SourceLocal.Offical);
        using var stream = await SendAsync(url);
        if (stream == null)
        {
            return;
        }
        var xml = new XmlDocument();
        xml.Load(stream);

        var list = new List<string>();

        var node = xml.SelectNodes("//metadata/versioning/versions/version");
        if (node == null)
        {
            return;
        }

        if (neo)
        {
            s_neoForgeVersion.Clear();
        }
        else
        {
            s_forgeVersion.Clear();
        }
        foreach (XmlNode item in node)
        {
            var str = item.InnerText;
            var args = str.Split('-');
            if (args.Length < 2)
            {
                continue;
            }
            var mc1 = args[0];
            var version = args[1];

            if (!list.Contains(mc1))
            {
                list.Add(mc1);
            }

            if (neo)
            {
                if (s_neoForgeVersion.TryGetValue(mc1, out var list2))
                {
                    list2.Add(new(version));
                }
                else
                {
                    var list3 = new List<string>() { version };
                    s_neoForgeVersion.Add(mc1, list3);
                }
            }
            else
            {
                if (s_forgeVersion.TryGetValue(mc1, out var list2))
                {
                    list2.Add(new(version));
                }
                else
                {
                    var list3 = new List<string>() { version };
                    s_forgeVersion.Add(mc1, list3);
                }
            }
        }

        foreach (var item in s_neoForgeVersion.Values)
        {
            StringHelper.VersionSort(item);
        }
        foreach (var item in s_forgeVersion.Values)
        {
            StringHelper.VersionSort(item);
        }

        if (neo)
        {
            s_neoSupportVersion = list;
        }
        else
        {
            s_supportVersion = list;
        }

        //新版neoforge
        if (neo)
        {
            url = UrlHelper.NeoForgeVersions(mc, true, SourceLocal.Offical);

            using var stream1 = await SendAsync(url);
            if (stream1 == null)
            {
                return;
            }

            xml = new XmlDocument();
            xml.Load(stream1);

            node = xml.SelectNodes("//metadata/versioning/versions/version");
            if (node?.Count > 0)
            {
                foreach (XmlNode item in node)
                {
                    var str = item.InnerText;
                    var args = str.Split('.');
                    var mc1 = "1." + args[0] + (args[1] == "0" ? "" : "." + args[1]);
                    var version = str;

                    if (!s_neoSupportVersion!.Contains(mc1))
                    {
                        s_neoSupportVersion.Add(mc1);
                    }

                    if (s_neoForgeVersion.TryGetValue(mc1, out var list2))
                    {
                        list2.Add(version);
                    }
                    else
                    {
                        var list3 = new List<string>() { version };
                        s_neoForgeVersion.Add(mc1, list3);
                    }
                }

                foreach (var item in s_neoForgeVersion.Values)
                {
                    StringHelper.VersionSort(item);
                }
            }
        }
    }
}
