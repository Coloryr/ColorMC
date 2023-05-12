using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml;

namespace ColorMC.Core.Utils;

public class Language
{
    private readonly Dictionary<string, string> LanguageList = new();

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

    public string GetLanguage(string key)
    {
        if (LanguageList.TryGetValue(key, out var res1))
        {
            return res1;
        }

        return key;
    }

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
