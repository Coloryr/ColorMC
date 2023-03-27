using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Optifine;
using ColorMC.Core.Utils;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

public static class OptifineHelper
{
    /// <summary>
    /// 获取高清修复版本
    /// </summary>
    public static async Task<(SourceLocal?, List<OptifineObj>?)> GetOptifineVersion()
    {
        string url = UrlHelper.Optifine(BaseClient.Source);
        try
        {
            var type = BaseClient.Source;
            var list = new List<OptifineObj>();
            var data = await BaseClient.GetString(url);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
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
                        Version = temp4,
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
                        Version = $"Optifine {item.mcversion} {item.patch}",
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
            ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Optifine.Error1"), e, false);
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
            _ = BaseClient.GetString(obj.Url1);
            var data = await BaseClient.GetString(obj.Url2);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(obj.Url2), false);
                return null;
            }
            HtmlDocument html = new();
            html.LoadHtml(data.Item2!);
            var list1 = html.DocumentNode.SelectNodes("//table/tr/td/table/tbody/tr/td/table/tbody/tr/td/span/a");
            if (list1 == null)
                return null;
            return "https://optifine.net/" + list1.First().Attributes["href"].Value;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Optifine.Error2"), e, false);
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
        if (item.Local == SourceLocal.Offical)
        {
            var data = await GetOptifineDownloadUrl(item);
            if (data == null)
            {
                return (false, LanguageHelper.GetName("Core.Http.Optifine.Error3"));
            }

            item1 = new()
            {
                Name = item.Version,
                Local = obj.GetModsPath() + "/" + item.FileName,
                Overwrite = true,
                Url = data
            };
        }
        else
        {
            item1 = new()
            {
                Name = item.Version,
                Local = obj.GetModsPath() + "/" + item.FileName,
                Overwrite = true,
                Url = item.Url1
            };
        }

        var res = await DownloadManager.Start(new() { item1 });
        if (!res)
        {
            return (false, LanguageHelper.GetName("Core.Http.Optifine.Error4"));
        }
        return (true, null);
    }
}
