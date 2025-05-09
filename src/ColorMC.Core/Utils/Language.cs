using System.Text.Json;

namespace ColorMC.Core.Utils;

/// <summary>
/// 语言储存
/// </summary>
public class Language
{
    private readonly Dictionary<string, string> _languageList = [];

    /// <summary>
    /// 加载语言
    /// </summary>
    /// <param name="item"></param>
    public void Load(Stream stream)
    {
        _languageList.Clear();
        var json = JsonDocument.Parse(stream);
        foreach (var item in json.RootElement.EnumerateObject())
        {
            _languageList.Add(item.Name, item.Value.GetString()!);
        }
    }

    /// <summary>
    /// 获取语言
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetLanguage(string key)
    {
        if (_languageList.TryGetValue(key, out var res1))
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
        if (_languageList.TryGetValue(key, out var res1))
        {
            have = true;
            return res1!;
        }

        have = false;
        return key;
    }
}
