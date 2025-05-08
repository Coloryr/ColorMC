using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏语言
/// </summary>
public static class GameLang
{
    /// <summary>
    /// 获取语言列表
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <returns>语言列表</returns>
    public static Task<Dictionary<string, string>> GetLangsAsync(this AssetsObj? obj)
    {
        return Task.Run(() =>
        {
            return GetLangs(obj);
        });
    }

    /// <summary>
    /// 获取语言列表
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <returns>语言列表</returns>
    public static Dictionary<string, string> GetLangs(this AssetsObj? obj)
    {
        var list = new Dictionary<string, string>();
        if (obj != null)
        {
#if false
            Parallel.ForEach(obj.Objects, new()
            {
                MaxDegreeOfParallelism = 1
            }, (item) =>
#else
            Parallel.ForEach(obj.Objects, (item) =>
#endif
            {
                if (!item.Key.StartsWith(Names.NameLangKey1))
                {
                    return;
                }

                try
                {
                    using var stream = AssetsPath.ReadAssets(item.Value.Hash);
                    if (stream == null)
                    {
                        return;
                    }
                    using var reader = new StreamReader(stream);
                    //json格式
                    var datas = new char[1];
                    var start = reader.ReadAsync(datas);
                    if (datas[0] == '{')
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        var data = JsonUtils.ToObj(stream, JsonType.LangObj);
                        if (data == null)
                        {
                            return;
                        }
                        list.Add(item.Key.Replace(Names.NameLangKey1, "").Replace(".json", ""),
                                data.Name + "-" + data.Region);
                    }
                    else
                    {
                        //key: value格式
                        string? name = null, region = null;
                        while (reader.ReadLine() is { } line)
                        {
                            if (line.StartsWith("language.name="))
                            {
                                name = line[14..].Trim();
                            }
                            else if (line.StartsWith("language.region="))
                            {
                                region = line[16..].Trim();
                            }
                            if (name != null && region != null)
                            {
                                break;
                            }
                        }

                        list.Add(item.Key.Replace(Names.NameLangKey1, "").Replace(".lang", ""),
                                   (name ?? "") + "-" + (region ?? ""));
                    }
                }
                catch
                {

                }
            });
        }

        //添加默认的语言选择
        list.TryAdd("zh_cn", "简体中文-中国大陆");
        list.TryAdd("en_us", "English-United States");

        var sortedByKey = list.OrderBy(kvp => kvp.Key);

        return sortedByKey.ToDictionary();
    }

    /// <summary>
    /// 获取语言列表
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <param name="key">资源键</param>
    /// <returns>语言键值</returns>
    public static LangRes? GetLang(this AssetsObj? obj, string key)
    {
        if (obj != null)
        {
            foreach (var item in obj.Objects)
            {
                try
                {
                    if (!item.Key.StartsWith(Names.NameLangKey1) || !item.Key.Contains(key))
                    {
                        continue;
                    }
                    using var stream = AssetsPath.ReadAssets(item.Value.Hash);
                    if (stream == null)
                    {
                        continue;
                    }
                    var data = JsonUtils.ToObj(stream, JsonType.LangObj);
                    if (data == null)
                    {
                        continue;
                    }
                    return new LangRes
                    {
                        Key = item.Key.Replace(Names.NameLangKey1, "").Replace(".json", ""),
                        Name = data.Name + "-" + data.Region
                    };
                }
                catch
                {

                }
            }
        }

        if (key == "zh_cn")
        {
            return new LangRes
            {
                Key = "zh_cn",
                Name = "简体中文"
            };
        }
        else if (key == "en_us")
        {
            return new LangRes
            {
                Key = "en_us",
                Name = "English"
            };
        }

        return null;
    }
}
