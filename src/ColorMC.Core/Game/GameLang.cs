using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.Minecraft;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏语音
/// </summary>
public static class GameLang
{
    public const string Name1 = "minecraft/lang/";
    public const string Name2 = "lang";

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
            foreach (var item in obj.objects)
            {
                if (item.Key.StartsWith(Name1) && AssetsPath.ReadAsset(item.Value.hash) is { } str)
                {
                    try
                    {
                        var data = JsonConvert.DeserializeObject<LangObj>(str)!;
                        list.Add(item.Key.Replace(Name1, "").Replace(".json", ""),
                                data.Name + "-" + data.Region);
                    }
                    catch
                    {

                    }
                }
            }
        }

        list.TryAdd("zh_cn", "简体中文");
        list.TryAdd("en_us", "English");

        return list;
    }

    /// <summary>
    /// 获取语言列表
    /// </summary>
    /// <param name="obj">资源数据</param>
    /// <returns>语言列表</returns>
    public static (string?, string?) GetLang(this AssetsObj? obj, string temp)
    {
        if (obj != null)
        {
            foreach (var item in obj.objects)
            {
                if (item.Key.StartsWith(Name1) && item.Key.Contains(temp) && AssetsPath.ReadAsset(item.Value.hash) is { } str)
                {
                    try
                    {
                        var data = JsonConvert.DeserializeObject<LangObj>(str)!;
                        return (item.Key.Replace(Name1, "").Replace(".json", ""),
                                data.Name + "-" + data.Region);
                    }
                    catch
                    {

                    }
                }
            }
        }

        //list.TryAdd("zh_cn", "简体中文");
        //list.TryAdd("en_us", "English");

        if (temp == "zh_cn")
        {
            return ("zh_cn", "简体中文");
        }
        else if (temp == "en_us")
        {
            return ("en_us", "English");
        }

        return (null, null);
    }
}
