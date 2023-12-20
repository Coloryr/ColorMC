using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Utils;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Optifine网络请求
/// </summary>
public static class OptifineAPI
{
    private static List<string> s_OptifneMcVersion;
    /// <summary>
    /// 获取高清修复版本
    /// </summary>
    public static async Task<(SourceLocal?, List<OptifineObj>?)> GetOptifineVersion()
    {
        string url = UrlHelper.GetOptifine(BaseClient.Source);
        try
        {
            var type = BaseClient.Source;
            var list = new List<OptifineObj>();
            var data = await BaseClient.GetStringAsync(url);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                    new Exception(url), false);
                return (null, null);
            }
            if (BaseClient.Source == SourceLocal.Offical)
            {
                HtmlDocument html = new();
                html.LoadHtml(data.Item2!);
                var list1 = html.DocumentNode.SelectNodes("//table/tr/td/span/div/table/tr");
                if (list1 == null)
                    return (null, null);

                foreach (var item in list1)
                {
                    var temp = item.Descendants("td")
                        .FirstOrDefault(a => a.Attributes.Contains("class")
                        && a.Attributes["class"].Value == "colDownload")?.FirstChild.Attributes["href"].Value;
                    var temp1 = item.Descendants("td")
                        .FirstOrDefault(a => a.Attributes.Contains("class")
                        && a.Attributes["class"].Value == "colMirror")?.FirstChild.Attributes["href"].Value;
                    var temp2 = item.Descendants("td")
                        .FirstOrDefault(a => a.Attributes.Contains("class")
                        && a.Attributes["class"].Value == "colForge")?.InnerText;
                    var temp3 = item.Descendants("td")
                        .FirstOrDefault(a => a.Attributes.Contains("class")
                        && a.Attributes["class"].Value == "colDate")?.InnerText;
                    var temp4 = item.Descendants("td")
                        .FirstOrDefault(a => a.Attributes.Contains("class")
                        && a.Attributes["class"].Value == "colFile")?.InnerText;

                    if (temp == null || temp1 == null || temp2 == null
                        || temp3 == null || temp4 == null)
                        continue;

                    string file = Path.GetFileName(temp1).Replace("adloadx?f=", "");
                    string mc = file.Replace("preview_OptiFine_", "")
                        .Replace("OptiFine_", "");
                    mc = mc[..(mc.IndexOf('_'))];

                    list.Add(new()
                    {
                        FileName = file,
                        Version = temp4.Replace("OptiFine ", "").Replace(" ", "_"),
                        MCVersion = mc,
                        Forge = temp2,
                        Date = temp3,
                        Url1 = temp,
                        Url2 = temp1,
                        Local = SourceLocal.Offical
                    });
                }
            }
            else
            {
                var list1 = JsonConvert.DeserializeObject<List<OptifineListObj>>(data.Item2!);

                if (list1 == null)
                    return (null, null);

                list1.ForEach(item =>
                {
                    list.Add(new()
                    {
                        FileName = item.filename,
                        Version = $"{item.type}_{item.patch}",
                        MCVersion = item.mcversion,
                        Forge = item.forge,
                        Url1 = UrlHelper.OptifineDownload(item, type),
                        Local = BaseClient.Source
                    });
                });
            }

            return (type, list);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.OptiFine.Error1"), e);
        }

        return (null, null);
    }

    /// <summary>
    /// 获取高清修复下载地址
    /// </summary>
    /// <param name="obj">高清修复信息</param>
    /// <returns>下载地址</returns>
    public static async Task<string?> GetOptifineDownloadUrl(OptifineObj obj)
    {
        try
        {
            if (obj.Local == SourceLocal.Offical)
            {
                _ = BaseClient.GetStringAsync(obj.Url1);
                var data = await BaseClient.GetStringAsync(obj.Url2);
                if (data.Item1 == false)
                {
                    ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                        new Exception(obj.Url2), false);
                    return null;
                }
                HtmlDocument html = new();
                html.LoadHtml(data.Item2!);
                var list1 = html.DocumentNode.SelectNodes("//table/tr/td/table/tbody/tr/td/table/tbody/tr/td/span/a");
                if (list1 == null)
                    return null;
                return UrlHelper.OptiFine + list1[0].Attributes["href"].Value;
            }
            else
            {
                return obj.Url1;
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.OptiFine.Error2"), e, false);
        }

        return null;
    }

    /// <summary>
    /// 下载高清修复
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="item">高清修复信息</param>
    /// <returns>结果</returns>
    public static async Task<(bool, string?)> DownloadOptifine(GameSettingObj obj, OptifineObj item)
    {
        DownloadItemObj item1;
        var data = await GetOptifineDownloadUrl(item);
        if (data == null)
        {
            return (false, LanguageHelper.Get("Core.Http.OptiFine.Error3"));
        }

        item1 = new()
        {
            Name = item.Version,
            Local = obj.GetModsPath() + "/" + item.FileName,
            Overwrite = true,
            Url = data
        };

        var res = await DownloadManager.StartAsync([item1]);
        if (!res)
        {
            return (false, LanguageHelper.Get("Core.Http.OptiFine.Error4"));
        }
        return (true, null);
    }

    public static async Task<List<string>?> GetSupportVersion()
    {
        if (s_OptifneMcVersion != null)
        {
            return s_OptifneMcVersion;
        }
        var list = await GetOptifineVersion();
        if (list.Item1 == null)
        {
            return null;
        }
        var list1 = new List<string>();
        var list2 = list.Item2!.GroupBy(item => item.MCVersion);
        foreach (var item in list2)
        {
            list1.Add(item.Key);
        }

        s_OptifneMcVersion = list1;

        return list1;
    }
}
