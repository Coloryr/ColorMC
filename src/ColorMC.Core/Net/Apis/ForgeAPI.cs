using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System.Xml;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Forge网络请求
/// </summary>
public static class ForgeAPI
{
    private static List<string>? s_supportVersion;
    private static readonly Dictionary<string, List<string>> s_forgeVersion = new();

    private static List<string>? s_neoSupportVersion;
    private static readonly Dictionary<string, List<string>> s_neoForgeVersion = new();

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

            if (local == SourceLocal.BMCLAPI
                || local == SourceLocal.MCBBS)
            {
                if (neo)
                {
                    s_neoSupportVersion = new() { "1.20.1" };
                    return s_neoSupportVersion;
                }
                string url = UrlHelper.ForgeVersion(local);
                var data = await BaseClient.GetString(url);
                if (data.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                        new Exception(url), false);
                    return null;
                }
                var obj = JsonConvert.DeserializeObject<List<string>>(data.Item2!);
                if (obj == null)
                    return null;

                if (neo)
                    s_neoSupportVersion = obj;
                else
                    s_supportVersion = obj;

                return obj;
            }
            else
            {
                await LoadFromSource(neo);

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
    /// <param name="local">下载源</param>
    /// <returns>版本列表</returns>
    public static async Task<List<string>?> GetVersionList(bool neo, string mc, SourceLocal? local = null)
    {
        try
        {
            List<string> list = new();
            if (local == SourceLocal.BMCLAPI
                || local == SourceLocal.MCBBS)
            {
                string url = neo
                    ? UrlHelper.NeoForgeVersions(mc, SourceLocal.Offical)
                    : UrlHelper.ForgeVersions(mc, local);
                var data = await BaseClient.GetString(url);
                if (data.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                        new Exception(url), false);
                    return null;
                }
                var list1 = new List<Version>();

                if (neo)
                {
                    var obj = JsonConvert.DeserializeObject<List<NeoForgeVersionObj>>(data.Item2!);
                    if (obj == null)
                        return null;

                    foreach (var item in obj)
                    {
                        list1.Add(new Version(item.version));
                    }
                }
                else
                {
                    var obj = JsonConvert.DeserializeObject<List<ForgeVersionObj1>>(data.Item2!);
                    if (obj == null)
                        return null;

                    foreach (var item in obj)
                    {
                        list1.Add(new Version(item.version));
                    }
                }

                list1.Sort();
                list1.Reverse();

                foreach (var item in list1)
                {
                    list.Add(item.ToString());
                }
                return list;
            }
            else
            {
                if (!neo && s_forgeVersion.TryGetValue(mc, out var list1))
                {
                    return list1;
                }
                else if (neo && s_neoForgeVersion.TryGetValue(mc, out list1))
                {
                    return list1;
                }

                await LoadFromSource(neo);

                if (neo)
                {
                    if (s_neoForgeVersion.TryGetValue(mc, out var list4))
                    {
                        list4.Reverse();
                        return list4;
                    }
                }
                else
                {
                    if (s_forgeVersion.TryGetValue(mc, out var list4))
                    {
                        list4.Reverse();
                        return list4;
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

    public static async Task LoadFromSource(bool neo)
    {
        string url = neo ?
                    UrlHelper.NeoForgeVersion(SourceLocal.Offical) :
                    UrlHelper.ForgeVersion(SourceLocal.Offical);
        var html = await BaseClient.GetString(url);
        if (html.Item1 == false)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                new Exception(url), false);
            return;
        }

        var xml = new XmlDocument();
        xml.LoadXml(html.Item2!);

        List<string> list = new();

        var node = xml.SelectNodes("//metadata/versioning/versions/version");
        if (node?.Count > 0)
        {
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
                var index = str.IndexOf('-');
                var mc1 = str[..index++];
                var version = str[index..];

                if (!list.Contains(mc1))
                {
                    list.Add(mc1);
                }

                if (neo)
                {
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
                else
                {
                    if (s_forgeVersion.TryGetValue(mc1, out var list2))
                    {
                        list2.Add(version);
                    }
                    else
                    {
                        var list3 = new List<string>() { version };
                        s_forgeVersion.Add(mc1, list3);
                    }
                }
            }

            if (neo)
                s_neoSupportVersion = list;
            else
                s_supportVersion = list;
        }
    }
}
