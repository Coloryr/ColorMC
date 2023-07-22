using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Linq;
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
                string url = neo ? 
                    UrlHelper.NeoForgeVersion(SourceLocal.Offical):
                    UrlHelper.ForgeVersion(SourceLocal.Offical);
                var html = await BaseClient.GetString(url);
                if (html.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                        new Exception(url), false);
                    return null;
                }

                var xml = new XmlDocument();
                xml.LoadXml(html.Item2!);

                List<string> list = new();

                var node = xml.SelectNodes("//metadata/versioning/versions/version");
                if (node?.Count > 0)
                {
                    foreach (XmlNode item in node)
                    {
                        var str = item.InnerText;
                        var index = str.IndexOf('-');
                        var mc = str[..index++];
                        var version = str[index..];

                        if (!list.Contains(mc))
                        {
                            list.Add(mc);
                        }
                    }

                    if (neo)
                        s_neoSupportVersion = list;
                    else
                        s_supportVersion = list;

                    return list;
                }
                else
                {
                    return null;
                }


                //var doc = new HtmlDocument();
                //doc.LoadHtml(html.Item2!);
                //var nodes = doc.DocumentNode.Descendants("li")
                //    .Where(x => x.Attributes["class"]?.Value == "li-version-list");
                //if (nodes == null)
                //    return null;
                //List<string> list = new();

                //foreach (var item in nodes)
                //{
                //    var nodes1 = item.SelectNodes("ul/li/a");
                //    if (nodes1 == null)
                //        return null;

                //    foreach (var item1 in nodes1)
                //    {
                //        list.Add(item1.InnerText.Trim());
                //    }

                //    var nodes2 = item.SelectNodes("ul/li")
                //        .Where(a => a.HasClass("elem-active"));

                //    foreach (var item1 in nodes2)
                //    {
                //        list.Add(item1.InnerText.Trim());
                //    }
                //}

                //SupportVersion = list;

                //return list;
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
    /// <param name="version">游戏版本</param>
    /// <param name="local">下载源</param>
    /// <returns>版本列表</returns>
    public static async Task<List<string>?> GetVersionList(bool neo, string version, SourceLocal? local = null)
    {
        try
        {
            List<string> list = new();
            if (local == SourceLocal.BMCLAPI
                || local == SourceLocal.MCBBS)
            {
                string url = UrlHelper.ForgeVersions(version, local);
                var data = await BaseClient.GetString(url);
                if (data.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                        new Exception(url), false);
                    return null;
                }
                var obj = JsonConvert.DeserializeObject<List<ForgeVersionObj1>>(data.Item2!);
                if (obj == null)
                    return null;

                var list1 = new List<Version>();
                foreach (var item in obj)
                {
                    list1.Add(new Version(item.version));
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
                if (!neo && s_forgeVersion.TryGetValue(version, out var list1))
                {
                    return list1;
                }
                else if (neo && s_neoForgeVersion.TryGetValue(version, out list1))
                {
                    return list1;
                }

                string url = neo ?
                    UrlHelper.NeoForgeVersions(version, SourceLocal.Offical) :
                    UrlHelper.ForgeVersions(version, SourceLocal.Offical);
                var data = await BaseClient.DownloadClient.GetAsync(url);

                string? html = null;
                if (data.IsSuccessStatusCode)
                {
                    html = await data.Content.ReadAsStringAsync();
                }
                if (string.IsNullOrWhiteSpace(html))
                {
                    return null;
                }

                var xml = new XmlDocument();
                xml.LoadXml(html);

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
                        var mc = str[..index++];
                        var version1 = str[index..];

                        if (neo)
                        {
                            if (s_neoForgeVersion.TryGetValue(mc, out var list2))
                            {
                                list2.Add(version1);
                            }
                            else
                            {
                                var list3 = new List<string>() { version1 };
                                s_neoForgeVersion.Add(mc, list3);
                            }
                        }
                        else
                        {
                            if (s_forgeVersion.TryGetValue(mc, out var list2))
                            {
                                list2.Add(version1);
                            }
                            else
                            {
                                var list3 = new List<string>() { version1 };
                                s_forgeVersion.Add(mc, list3);
                            }
                        }
                    }

                    if (neo)
                    {
                        if (s_neoForgeVersion.TryGetValue(version, out var list4))
                        {
                            return list4;
                        }
                    }
                    else
                    {
                        if (s_forgeVersion.TryGetValue(version, out var list4))
                        {
                            return list4;
                        }
                    }
                }

                return null;

                //var doc = new HtmlDocument();
                //doc.LoadHtml(html);
                //var nodes = doc.DocumentNode.Descendants("table")
                //    .Where(x => x.Attributes["class"]?.Value == "download-list").FirstOrDefault();
                //if (nodes == null)
                //    return null;
                //var nodes1 = nodes.Descendants("tbody").FirstOrDefault();
                //if (nodes1 == null)
                //    return null;

                //foreach (var item in nodes1.Descendants("tr"))
                //{
                //    var item1 = item.Descendants("td").Where(x => x.Attributes["class"]?.Value == "download-version").FirstOrDefault();
                //    if (item1 != null)
                //    {
                //        string item2 = item1.InnerText.Trim();
                //        if (item2.Contains("Branch:"))
                //        {
                //            int a = item2.IndexOf(' ');
                //            item2 = item2[..a].Trim();
                //        }
                //        list.Add(item2);
                //    }
                //}
                //return list;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Forge.Error6"), e);
            return null;
        }
    }
}
