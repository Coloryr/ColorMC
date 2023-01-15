using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace ColorMC.Core.Game;

public static class Mods
{
    public static async Task<List<ModObj>> GetMods(this GameSettingObj obj)
    {
        var list = new List<ModObj>();
        string dir = obj.GetModsPath();

        DirectoryInfo info = new(dir);
        if (!info.Exists)
        {
            return list;
        }

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = 10
        };
        await Parallel.ForEachAsync(info.GetFiles(), options, async (item, cancel) =>
        {
            if (item.Extension is not (".jar" or ".disable"))
                return;
            try
            {
                using ZipFile zFile = new(item.FullName);
                var item1 = zFile.GetEntry("mcmod.info");
                if (item1 != null)
                {
                    using var stream1 = zFile.GetInputStream(item1);
                    using var stream = new MemoryStream();
                    await stream1.CopyToAsync(stream, cancel);
                    var data = Encoding.UTF8.GetString(stream.ToArray());
                    if (data.StartsWith("{"))
                    {
                        var obj1 = JObject.Parse(data);
                        var obj2 = obj1.GetValue("modList") as JArray;
                        if (obj2?.Count > 0)
                        {
                            var obj3 = obj2.First().ToObject<ModObj>()!;
                            obj3.V2 = false;
                            obj3.Local = Path.GetFullPath(item.FullName);
                            obj3.Disable = item.Extension is ".disable";
                            obj3.Loaders = Loaders.Forge;
                            list.Add(obj3);
                        }
                    }
                    else if (data.StartsWith("["))
                    {
                        var obj1 = JArray.Parse(data);
                        if (obj1?.Count > 0)
                        {
                            var obj3 = obj1.First().ToObject<ModObj>()!;
                            obj3.V2 = false;
                            obj3.Local = Path.GetFullPath(item.FullName);
                            obj3.Disable = item.Extension is ".disable";
                            obj3.Loaders = Loaders.Forge;
                            list.Add(obj3);
                        }
                    }
                    return;
                }

                item1 = zFile.GetEntry("META-INF/mods.toml");
                if (item1 != null)
                {
                    using var stream1 = zFile.GetInputStream(item1);
                    using var stream = new MemoryStream();
                    await stream1.CopyToAsync(stream, cancel);
                    var model = Toml.Parse(stream.ToArray()).ToModel();
                    if (model["mods"] is not TomlTableArray model1)
                        return;
                    var model2 = model1[0];
                    if (model2 == null)
                        return;
                    ModObj obj3 = new()
                    {
                        V2 = true,
                        Loaders = Loaders.Forge,
                        Local = Path.GetFullPath(item.FullName),
                        Disable = item.Extension is ".disable"
                    };
                    model2.TryGetValue("modId", out object item2);
                    obj3.modid = item2 as string;
                    model2.TryGetValue("displayName", out item2);
                    obj3.name = item2 as string;
                    model2.TryGetValue("modId", out item2);
                    obj3.modid = item2 as string;
                    model2.TryGetValue("description", out item2);
                    obj3.description = item2 as string;
                    model2.TryGetValue("version", out item2);
                    obj3.version = item2 as string;
                    model2.TryGetValue("authorList", out item2);
                    obj3.authorList = (item2 as string).ToStringList();
                    model2.TryGetValue("displayURL", out item2);
                    obj3.url = item2 as string;

                    list.Add(obj3);
                    return;
                }

                item1 = zFile.GetEntry("fabric.mod.json");
                if (item1 != null)
                {
                    using var stream1 = zFile.GetInputStream(item1);
                    using var stream = new MemoryStream();
                    await stream1.CopyToAsync(stream, cancel);
                    var data = Encoding.UTF8.GetString(stream.ToArray());
                    var obj1 = JObject.Parse(data);
                    var obj3 = new ModObj
                    {
                        Local = Path.GetFullPath(item.FullName),
                        Disable = item.Extension is ".disable",
                        Loaders = Loaders.Fabric,
                        V2 = true,
                        modid = obj1["id"].ToString(),
                        name = obj1["name"].ToString(),
                        description = obj1["description"].ToString(),
                        version = obj1["version"].ToString(),
                        authorList = (obj1["authors"] as JArray).ToStringList(),
                        url = obj1["contact"]?["homepage"]?.ToString()
                    };
                    list.Add(obj3);
                    return;
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.GetName("Core.Game.Error1"), e);
            }
        });


        list.Sort(new ModComparer());

        return list;
    }

    public static void Disable(this ModObj mod)
    {
        if (mod.Disable)
            return;

        var file = new FileInfo(mod.Local);
        mod.Disable = true;
        mod.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name.Replace(".jar", ".disable")}");
        File.Move(file.FullName, mod.Local);
    }

    public static void Enable(this ModObj mod)
    {
        if (!mod.Disable)
            return;

        var file = new FileInfo(mod.Local);
        mod.Disable = false;
        mod.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name.Replace(".disable", ".jar")}");
        File.Move(file.FullName, mod.Local);
    }

    public static void Delete(this ModObj mod)
    {
        File.Delete(mod.Local);
    }

    private static List<string> ToStringList(this string obj)
    {
        List<string> list = new();
        if (obj == null)
            return list;
        foreach (var item in obj.Split(","))
        {
            list.Add(item.Trim());
        }
        return list;
    }

    private static List<string> ToStringList(this JArray array)
    {
        List<string> list = new();
        foreach (var item in array)
        {
            list.Add(item.ToString());
        }

        return list;
    }


    class ModComparer : IComparer<ModObj>
    {
        public int Compare(ModObj? x, ModObj? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }
            if (x.name != y.name)
            {
                return x.name.CompareTo(y.name);
            }
            else return 0;
        }
    }
}
