using ColorMC.Core.Objs;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ColorMC.Core.Utils;

public static class LocalMaven
{
    private static readonly ConcurrentDictionary<string, MavenItemObj> Items = new();

    public const string Name1 = "maven.json";
    private static string Dir;
    public static void Init(string dir)
    {
        Dir = dir + Name1;

        if (File.Exists(Dir))
        {
            try
            {
                var data = File.ReadAllText(Dir);
                var list = JsonConvert.DeserializeObject<Dictionary<string, MavenItemObj>>(data);

                if (list != null)
                {
                    Items.Clear();

                    foreach (var item in list)
                    {
                        Items.TryAdd(item.Key, item.Value);
                    }
                }
            }
            catch
            {

            }
        }
    }

    public static MavenItemObj? GetItem(string name)
    {
        if (Items.TryGetValue(name, out var item))
        {
            return item;
        }

        return null;
    }

    public static void AddItem(MavenItemObj item)
    {
        if (Items.ContainsKey(item.Name))
        {
            Items[item.Name] = item;
        }
        else
        {
            Items.TryAdd(item.Name, item);
        }

        ConfigSave.AddItem(new()
        {
            Name = Name1,
            Local = Dir,
            Obj = Items
        });
    }
}
