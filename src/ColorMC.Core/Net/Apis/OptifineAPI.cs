using System.Net;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Utils;
using HtmlAgilityPack;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Optifine网络请求
/// </summary>
public static class OptifineAPI
{
    /// <summary>
    /// optifine支持的游戏版本
    /// </summary>
    private static List<string> s_OptifneMcVersion;

    /// <summary>
    /// 获取高清修复版本
    /// </summary>
    public static async Task<List<OptifineObj>?> GetOptifineVersion()
    {
        string url = UrlHelper.GetOptifine(CoreHttpClient.Source);
        try
        {
            var list = new List<OptifineObj>();
            var data = await CoreHttpClient.GetAsync(url);
            if (data.StatusCode != HttpStatusCode.OK)
            {
                ColorMCCore.OnError(LanguageHelper.Get("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }
            using var stream = await data.Content.ReadAsStreamAsync();
            if (CoreHttpClient.Source == SourceLocal.Offical)
            {
                var html = new HtmlDocument();
                html.Load(stream);
                var list2 = html.DocumentNode.SelectNodes("//tr");
                var list1 = list2?.Where(item => item?.GetClasses()?.Contains("downloadLine") == true);
                if (list1 == null)
                {
                    return null;
                }

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

                    var temp5 = temp3.Split(".");
                    if (temp5.Length == 3 && temp5[2].Length == 4)
                    {
                        temp3 = $"{temp5[2]}.{temp5[1]}.{temp5[0]}";
                    }

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
                var list1 = JsonUtils.ToObj(stream, JsonType.ListOptifineListObj);

                if (list1 == null)
                {
                    return null;
                }

                list1.ForEach(item =>
                {
                    list.Add(new()
                    {
                        FileName = item.Filename,
                        Version = $"{item.Type}_{item.Patch}",
                        MCVersion = item.Mcversion,
                        Forge = item.Forge,
                        Url1 = UrlHelper.OptifineDownload(item, CoreHttpClient.Source),
                        Local = CoreHttpClient.Source
                    });
                });
            }

            return list;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.OptiFine.Error1"), e);
            return null;
        }
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
                _ = CoreHttpClient.GetStringAsync(obj.Url1);
                var data = await CoreHttpClient.GetStringAsync(obj.Url2);
                if (data.State == false)
                {
                    ColorMCCore.OnError(LanguageHelper.Get("Core.Http.Error7"),
                        new Exception(obj.Url2), false);
                    return null;
                }
                HtmlDocument html = new();
                html.LoadHtml(data.Message!);
                var list1 = html.DocumentNode.SelectNodes("//table/tr/td/table/tbody/tr/td/table/tbody/tr/td/span/a");
                if (list1 == null)
                    return null;
                obj.Url1 = UrlHelper.OptiFine + list1[0].Attributes["href"].Value;
                obj.Url2 = "";
                obj.Local = SourceLocal.BMCLAPI;
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Http.OptiFine.Error2"), e, false);
        }

        return null;
    }

    /// <summary>
    /// 下载高清修复
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="item">高清修复信息</param>
    /// <returns>结果</returns>
    public static async Task<MessageRes> DownloadOptifine(GameSettingObj obj, OptifineObj item)
    {
        FileItemObj item1;
        var data = await GetOptifineDownloadUrl(item);
        if (data == null)
        {
            return new() { Message = LanguageHelper.Get("Core.Http.OptiFine.Error3") };
        }

        item1 = new()
        {
            Name = item.Version,
            Local = obj.GetModsPath() + "/" + item.FileName,
            Overwrite = true,
            Url = data
        };

        VersionPath.AddOptifine(item);

        var res = await DownloadManager.StartAsync([item1]);
        if (!res)
        {
            return new() { Message = LanguageHelper.Get("Core.Http.OptiFine.Error4") };
        }
        return new() { State = true };
    }

    /// <summary>
    /// 获取支持的游戏版本
    /// </summary>
    /// <returns></returns>
    public static async Task<List<string>?> GetSupportVersion()
    {
        if (s_OptifneMcVersion != null)
        {
            return s_OptifneMcVersion;
        }
        var list = await GetOptifineVersion();
        if (list == null)
        {
            return null;
        }
        var list1 = new List<string>();
        var list2 = list.GroupBy(item => item.MCVersion);
        foreach (var item in list2)
        {
            list1.Add(item.Key);
        }

        s_OptifneMcVersion = list1;

        return list1;
    }
}
