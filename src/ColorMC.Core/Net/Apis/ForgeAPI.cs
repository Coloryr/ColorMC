using System.Xml;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
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

    /// <summary>
    /// 获取支持的版本
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>?> GetSupportVersionAsync(bool neo, SourceLocal? local = null)
    {
        if ((neo ? s_neoSupportVersion : s_supportVersion) != null)
        {
            return neo ? s_neoSupportVersion : s_supportVersion;
        }

        if (local == SourceLocal.BMCLAPI)
        {
            if (neo)
            {
                if (s_neoSupportVersion == null)
                {
                    await LoadFromSourceAsync(true);
                }
                return s_neoSupportVersion;
            }
            else
            {
                return await GetSupportVersionBmclAsync(neo, local);
            }
        }
        else
        {
            await LoadFromSourceAsync(neo);

            return neo ? s_neoSupportVersion : s_supportVersion;
        }
    }

    private static async Task<List<string>?> GetSupportVersionBmclAsync(bool neo, SourceLocal? local = null)
    {
        string url = UrlHelper.ForgeVersion(local);
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
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
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return null;
        }
    }

    /// <summary>
    /// 获取版本列表
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="source">下载源</param>
    /// <returns>版本列表</returns>
    public static async Task<List<string>?> GetVersionListAsync(bool neo, string mc, SourceLocal? source = null)
    {
        bool v222 = CheckHelpers.IsGameVersion1202(mc);
        if (source == SourceLocal.BMCLAPI)
        {
            string url = neo
                    ? UrlHelper.NeoForgeVersions(mc, v222, source)
                    : UrlHelper.ForgeVersions(mc, source);
            try
            {
                using var stream = await CoreHttpClient.GetStreamAsync(url);

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
            catch (Exception e)
            {
                ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
                return null;
            }
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

            await LoadFromSourceAsync(neo);

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

    /// <summary>
    /// 从xml获取信息
    /// </summary>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns></returns>
    private static async Task LoadFromSourceAsync(bool neo)
    {
        var url = neo ? UrlHelper.NeoForgeVersions("", false, SourceLocal.Offical) :
                    UrlHelper.ForgeVersion(SourceLocal.Offical);
        //旧版neoforge与forge
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
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
            if (neo)
            {
                foreach (var item in s_neoForgeVersion.Values)
                {
                    StringHelper.VersionSort(item);
                }
            }
            else
            {
                foreach (var item in s_forgeVersion.Values)
                {
                    StringHelper.VersionSort(item);
                }
            }

            if (neo)
            {
                s_neoSupportVersion = list;
            }
            else
            {
                s_supportVersion = list;
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
        }

        //新版neoforge
        if (neo)
        {
            url = UrlHelper.NeoForgeVersions("", true, SourceLocal.Offical);
            try
            {
                using var stream1 = await CoreHttpClient.GetStreamAsync(url);
                if (stream1 == null)
                {
                    return;
                }

                var xml = new XmlDocument();
                xml.Load(stream1);

                var node = xml.SelectNodes("//metadata/versioning/versions/version");
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
            catch (Exception e)
            {
                ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            }
        }
    }
}
