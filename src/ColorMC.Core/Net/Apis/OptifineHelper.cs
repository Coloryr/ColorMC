using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Optifine;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Apis;

public static class OptifineHelper
{
    public static async Task<(SourceLocal?, List<OptifineObj>?)> GetOptifineVersion()
    {
        string url = UrlHelper.Optifine(BaseClient.Source);
        try
        {
            var type = BaseClient.Source;
            var list = new List<OptifineObj>();
            string data = await BaseClient.GetString(url);

            if (BaseClient.Source == SourceLocal.Offical)
            {
                HtmlDocument html = new();
                html.LoadHtml(data);
                var list1 = html.DocumentNode.SelectNodes("//table/tr/td/span/div/table/tr");
                if (list1 == null)
                    return (null, null);

                foreach (var item in list1)
                {
                    var temp = item.Descendants("td")
                        .FirstOrDefault(a=>a.Attributes.Contains("class") 
                        && a.Attributes["class"].Value == "colDownload")?.FirstChild.Attributes["href"].Value;
                    var temp1 = item.Descendants("td")
                        .FirstOrDefault(a => a.Attributes.Contains("class")
                        && a.Attributes["class"].Value == "colMirror" )?.FirstChild.Attributes["href"].Value;
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
                        Url2 = temp1
                    });
                }
            }
            else
            {
                var list1 = JsonConvert.DeserializeObject<List<OptifineListObj>>(data);

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
                        Url1 = UrlHelper.OptifineDownload(item, type)
                    });
                });
            }

            return (type, list);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("Optifine数据加载失败", e, false);
        }

        return (null, null);
    }

    public static async Task<string?> GetOptifineDownloadUrl(OptifineObj obj)
    {
        try
        {
            _ = BaseClient.GetString(obj.Url1);
            var data = await BaseClient.GetString(obj.Url2);

            HtmlDocument html = new();
            html.LoadHtml(data);
            var list1 = html.DocumentNode.SelectNodes("//table/tr/td/table/tbody/tr/td/table/tbody/tr/td/span/a");
            if (list1 == null)
                return null;
            return "https://optifine.net/" + list1.FirstOrDefault().Attributes["href"].Value;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("Optifine下载地址获取失败", e, false);
        }

        return null;
    }
}
