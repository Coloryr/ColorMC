using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Utils;

/// <summary>
/// 语言储存
/// </summary>
public class Language
{
    private readonly Dictionary<string, string> LanguageList = new();

    /// <summary>
    /// 加载语言
    /// </summary>
    /// <param name="item"></param>
    public void Load(Stream item)
    {
        LanguageList.Clear();
        using var steam = new StreamReader(item);
        var json = JObject.Parse(steam.ReadToEnd());
        foreach (JProperty item1 in json.Properties())
        {
            LanguageList.Add(item1.Name, item1.Value.ToString());
        }
    }

    /// <summary>
    /// 获取语言
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetLanguage(string key)
    {
        if (LanguageList.TryGetValue(key, out var res1))
        {
            return res1;
        }

        return key;
    }

    /// <summary>
    /// 获取语言
    /// </summary>
    /// <param name="key"></param>
    /// <param name="have"></param>
    /// <returns></returns>
    public string GetLanguage(string key, out bool have)
    {
        if (LanguageList.TryGetValue(key, out var res1))
        {
            have = true;
            return res1!;
        }

        have = false;
        return key;
    }
}
