using System.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Java;
using ColorMC.Core.Utils;
using HtmlAgilityPack;
using Jint;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// OpenJ9下载源
/// </summary>
public static class OpenJ9Api
{
    public const string Url = "https://developer.ibm.com/middleware/v1/contents/static/semeru-runtime-downloads";
    /// <summary>
    /// 获取列表
    /// </summary>
    public static async Task<GetOpenJ9ListRes?> GetJavaList()
    {
        using var data = await CoreHttpClient.GetAsync(Url);
        if (data.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }
        using var stream = await data.Content.ReadAsStreamAsync();
        var obj = JsonUtils.ToObj(stream, JsonType.OpenJ9Obj);
        if (obj == null || obj.Error)
        {
            return null;
        }

        var item1 = obj.Results[0].Content;
        var item2 = obj.Results[0].PagepostCustomJsValue;

        var doc = new HtmlDocument();
        doc.LoadHtml(item1);

        var nodes = doc.DocumentNode.Descendants("select")
            .Where(x => x.Attributes["id"]?.Value == "runtimeVersion");
        if (!nodes.Any())
        {
            return null;
        }

        var node = nodes.ToList()[0];
        var nodes1 = node.SelectNodes("option")?
            .Where(x => x.Attributes["class"]?.Value == "bx--select-option");
        if (nodes1 == null)
        {
            return null;
        }
        var mainversion = new List<string>()
        {
            ""
        };
        foreach (var item in nodes1)
        {
            mainversion.Add(item.Attributes["value"].Value);
        }

        nodes = doc.DocumentNode.Descendants("select")
            .Where(x => x.Attributes["id"]?.Value == "operatingSystem");
        if (!nodes.Any())
        {
            return null;
        }
        node = nodes.ToList()[0];
        nodes1 = node.SelectNodes("option");
        if (nodes1 == null)
        {
            return null;
        }
        var system = new List<string>()
        {
            ""
        };
        
        foreach (var item in nodes1)
        {
            string value = item.Attributes["value"].Value;
            if (value.Equals("any", StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }
            system.Add(value);
        }

        nodes = doc.DocumentNode.Descendants("select")
            .Where(x => x.Attributes["id"]?.Value == "arch");
        if (!nodes.Any())
        {
            return null;
        }
        node = nodes.ToList()[0];
        nodes1 = node.SelectNodes("option");
        if (nodes1 == null)
        {
            return null;
        }
        var arch = new List<string>()
        {
            ""
        };
        foreach (var item in nodes1)
        {
            string value = item.Attributes["value"].Value;
            if (value.Equals("any", StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }
            arch.Add(value);
        }

        var temp = item2.IndexOf("function");
        item2 = item2[..temp];
        item2 += Environment.NewLine;
        item2 += "JSON.stringify(sourceDataJson)";

        using var engine = new Engine();
        var obj1 = engine.Evaluate(item2);
        if (obj1 == null)
        {
            return null;
        }

        var obj2 = JsonUtils.ToObj(obj1.ToString(), JsonType.OpenJ9FileObj);
        if (obj2 == null)
        {
            return null;
        }

        return new()
        {
            Arch = arch,
            Os = system,
            MainVersion = mainversion,
            Download = obj2.Downloads
        };
    }
}
