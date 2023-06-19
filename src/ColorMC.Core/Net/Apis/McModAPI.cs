using ColorMC.Core.Objs;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Utils;
using HtmlAgilityPack;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// MC百科网络请求
/// </summary>
public static class McModAPI
{
    private static async Task<List<McModSearchItemObj>?> Search(string key, int page, int filter)
    {
        try
        {
            var url = $"https://search.mcmod.cn/s?key={key}&page={page}&filter={filter}";
            var data = await BaseClient.GetString(url);
            if (!data.Item1)
            {
                return null;
            }
            var list = new List<McModSearchItemObj>();
            var doc = new HtmlDocument();
            doc.LoadHtml(data.Item2);
            var nodes = doc.DocumentNode.Descendants("div")
                    .Where(x => x.Attributes["class"]?.Value == "result-item");
            foreach (var item in nodes)
            {
                var head = item.SelectNodes("div/a").First();
                var url1 = head.Attributes["href"]?.Value!;
                if (filter == 0 && !url1.StartsWith("https://www.mcmod.cn/class"))
                {
                    continue;
                }
                var body = item.SelectNodes("div").Where(a => a.Attributes["class"]?.Value == "body").First();
                var time = item.SelectNodes("div/span/span");
                var time1 = "";
                bool next = false;
                foreach (var item1 in time)
                {
                    if (next)
                    {
                        time1 = item1.InnerText;
                        break;
                    }
                    if (item1.FirstChild.InnerText.StartsWith("快照时间"))
                    {
                        next = true;
                    }
                }
                list.Add(new()
                {
                    Name = head.InnerText,
                    Text = body.InnerText,
                    Url = url1,
                    Type = filter == 1 ? FileType.Mod : FileType.ModPack,
                    Time = time1
                });
            }
            return list;
        }
        catch (Exception e)
        {
            Logs.Error("mcmod error", e);
            return null;
        }
    }

    public static Task<List<McModSearchItemObj>?> SearchMod(string key, int page)
    {
        return Search(key, page, 0);
    }

    public static Task<List<McModSearchItemObj>?> SearchModPack(string key, int page)
    {
        return Search(key, page, 2);
    }
}
