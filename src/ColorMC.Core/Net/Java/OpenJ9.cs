using ColorMC.Core.Objs.Java;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Java;

public static class OpenJ9
{
    public static async Task<(List<string>?, List<string>?, List<object>?)> GetJavaList()
    {
        var url = "https://developer.ibm.com/middleware/v1/contents/static/semeru-runtime-downloads";
        var data = await BaseClient.DownloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (data == null)
        {
            return (null, null, null);
        }
        var str = await data.Content.ReadAsStringAsync();

        var obj = JsonConvert.DeserializeObject<OpenJ9Obj>(str);
        if (obj == null || obj.error)
        {
            return (null, null, null);
        }

        var item1 = obj.results[0].content;
        var item2 = obj.results[0].pagepost_custom_js_value;

        var doc = new HtmlDocument();
        doc.LoadHtml(item1);

        var nodes = doc.DocumentNode.Descendants("select")
            .Where(x => x.Attributes["id"]?.Value == "runtimeVersion");
        if (!nodes.Any())
        {
            return (null, null, null);
        }

        var node = nodes.First();
        var nodes1 = node.SelectNodes("option")
            .Where(x => x.Attributes["class"]?.Value == "bx--select-option");

        var mainversion = new List<string>();
        foreach(var item in nodes1)
        {
            mainversion.Add(item.Attributes["value"].Value);    
        }

        nodes = doc.DocumentNode.Descendants("select")
            .Where(x => x.Attributes["id"]?.Value == "operatingSystem");
        if (!nodes.Any())
        {
            return (null, null, null);
        }
        node = nodes.First();
        nodes1 = node.SelectNodes("option");

        var system = new List<string>();
        foreach (var item in nodes1)
        {
            string value = item.Attributes["value"].Value;
            if (value.ToLower() == "any")
                continue;
            system.Add(value);
        }

        nodes = doc.DocumentNode.Descendants("select")
            .Where(x => x.Attributes["id"]?.Value == "arch");
        if (!nodes.Any())
        {
            return (null, null, null);
        }
        node = nodes.First();
        nodes1 = node.SelectNodes("option");

        var arch = new List<string>();
        foreach (var item in nodes1)
        {
            string value = item.Attributes["value"].Value;
            if (value.ToLower() == "any")
                continue;
            arch.Add(value);
        }

        return (null, null, null);
    }
}
