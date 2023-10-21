using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.Minecraft;
using Jint.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Game;

public static class GameLang
{
    public const string Name1 = "minecraft/lang/";
    public const string Name2 = "lang";
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

        if (!list.ContainsKey("zh_cn"))
        {
            list.Add("zh_cn", "简体中文");
        }
        if (!list.ContainsKey("en_us"))
        {
            list.Add("en_us", "English");
        }

        return list;
    }
}
