using System.Xml;

namespace ColorMC.Core.Utils;

public class Language
{
    private readonly Dictionary<string, string> LanguageList = new();

    public void Load(Stream item)
    {
        LanguageList.Clear();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(item);
        foreach (XmlNode item1 in xmlDoc.DocumentElement!.ChildNodes)
        {
            if (item1.Name == "String")
            {
                LanguageList.Add(item1.Attributes!.GetNamedItem("Key")!.Value!,
                    item1.FirstChild!.Value!);
            }
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
