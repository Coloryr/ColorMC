using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace ColorMC.Core.Game;

public static class Mods
{
    /// <summary>
    /// 获取Mod列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task<List<ModObj>> GetMods(this GameSettingObj obj)
    {
        var list = new ConcurrentBag<ModObj>();
        string dir = obj.GetModsPath();

        DirectoryInfo info = new(dir);
        if (!info.Exists)
        {
            info.Create();
            return list.ToList();
        }
        var files = info.GetFiles();

        //多线程同时检查
        await Parallel.ForEachAsync(files, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 1
        }, async (item, cancel) =>
        {
            if (item.Extension is not (".zip" or ".jar" or ".disable"))
                return;
            string sha1 = "";
            bool find = false;
            try
            {
                {
                    using var stream = File.OpenRead(item.FullName);
                    sha1 = Funtcions.GenSha1(stream);
                }

                //Mod 资源包
                if (item.Extension is ".zip")
                {
                    var obj3 = new ModObj
                    {
                        Local = Path.GetFullPath(item.FullName),
                        Disable = item.Extension is ".disable",
                        Loader = Loaders.Fabric,
                        V2 = true,
                        name = item.Name,
                        Sha1 = sha1
                    };
                    list.Add(obj3);
                    find = true;
                    return;
                }

                using ZipFile zFile = new(item.FullName);

                //forge 1.13以下
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
                            obj3.name ??= LanguageHelper.GetName("Core.Game.Error6");
                            obj3.V2 = false;
                            obj3.Local = Path.GetFullPath(item.FullName);
                            obj3.Disable = item.Extension is ".disable";
                            obj3.Loader = Loaders.Forge;
                            obj3.Sha1 = sha1;
                            obj3.Game = obj;
                            list.Add(obj3);
                            find = true;
                            return;
                        }
                    }
                    else if (data.StartsWith("["))
                    {
                        var obj1 = JArray.Parse(data);
                        if (obj1?.Count > 0)
                        {
                            var obj3 = obj1.First().ToObject<ModObj>()!;
                            obj3.name ??= LanguageHelper.GetName("Core.Game.Error6");
                            obj3.V2 = false;
                            obj3.Local = Path.GetFullPath(item.FullName);
                            obj3.Disable = item.Extension is ".disable";
                            obj3.Loader = Loaders.Forge;
                            obj3.Sha1 = sha1;
                            obj3.Game = obj;
                            list.Add(obj3);
                            find = true;
                            return;
                        }
                    }
                }

                //forge coremod
                item1 = zFile.GetEntry("META-INF/fml_cache_annotation.json");
                if (item1 != null)
                {
                    using var stream1 = zFile.GetInputStream(item1);
                    using var stream = new MemoryStream();
                    await stream1.CopyToAsync(stream, cancel);
                    var data = Encoding.UTF8.GetString(stream.ToArray());
                    var obj1 = JObject.Parse(data);
                    var obj2 = FindKey(obj1, "acceptedMinecraftVersions");
                    if (obj2 == null)
                        return;

                    ModObj obj3 = new()
                    {
                        V2 = true,
                        Loader = Loaders.Forge,
                        Local = Path.GetFullPath(item.FullName),
                        Disable = item.Extension is ".disable",
                        Game = obj,
                        modid = obj2["modId"]?["value"]?.ToString(),
                        name = obj2["names"]?["value"]?.ToString(),
                        version = obj2["version"]?["value"]?.ToString(),
                        dependants = new() { obj2["dependencies"]?["value"]?.ToString() },
                        Sha1 = sha1
                    };

                    obj3.name ??= obj3.modid;

                    list.Add(obj3);
                    find = true;
                    return;
                }

                //forge 1.13及以上
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
                        Loader = Loaders.Forge,
                        Local = Path.GetFullPath(item.FullName),
                        Disable = item.Extension is ".disable",
                        Game = obj
                    };
                    model2.TryGetValue("modId", out object item2);
                    obj3.modid = item2 as string;
                    model2.TryGetValue("displayName", out item2);
                    obj3.name = item2 as string ??
                            LanguageHelper.GetName("Core.Game.Error6");
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
                    obj3.Sha1 = sha1;

                    list.Add(obj3);
                    find = true;
                    return;
                }

                //fabric
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
                        Loader = Loaders.Fabric,
                        V2 = true,
                        modid = obj1["id"]?.ToString(),
                        name = obj1["name"]?.ToString() ??
                            LanguageHelper.GetName("Core.Game.Error6"),
                        description = obj1["description"]?.ToString(),
                        version = obj1["version"]?.ToString(),
                        authorList = (obj1["authors"] as JArray)?.ToStringList(),
                        url = obj1["contact"]?["homepage"]?.ToString(),
                        Game = obj,
                        Sha1 = sha1
                    };
                    list.Add(obj3);
                    find = true;
                    return;
                }

                //quilt
                item1 = zFile.GetEntry("quilt.mod.json");
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
                        Loader = Loaders.Quilt,
                        V2 = true,
                        modid = obj1["quilt_loader"]?["id"]?.ToString(),
                        name = obj1["quilt_loader"]?["metadata"]?["name"]?.ToString() ??
                            LanguageHelper.GetName("Core.Game.Error6"),
                        description = obj1["quilt_loader"]?["metadata"]?["description"]?.ToString(),
                        version = obj1["quilt_loader"]?["version"]?.ToString(),
                        authorList = (obj1["quilt_loader"]?["metadata"]?["contributors"] as JObject)?.ToStringList(),
                        url = obj1["quilt_loader"]?["contact"]?["homepage"]?.ToString(),
                        Sha1 = sha1,
                        Game = obj
                    };
                    list.Add(obj3);
                    find = true;
                    return;
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.GetName("Core.Game.Error1"), e);
            }
            if (!find)
            {
                list.Add(new()
                {
                    name = "",
                    Local = Path.GetFullPath(item.FullName),
                    Disable = item.Extension is ".disable",
                    Broken = true,
                    Sha1 = sha1,
                    Game = obj
                });
            }
        });

        var list1 = list.ToList();
        list1.Sort(ModComparer.Instance);

        return list1;
    }

    /// <summary>
    /// 找到指定obj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key">键名</param>
    /// <returns></returns>
    private static JContainer? FindKey(this JObject obj, string key)
    {
        foreach (var item in obj)
        {
            if (item.Key == key)
            {
                return item.Value?.Parent;
            }

            if (item.Value is JObject obj1)
            {
                return FindKey(obj1, key);
            }
            else if (item.Value is JArray arry)
            {
                return FindKey(arry, key);
            }
        }

        return null;
    }

    /// <summary>
    /// 找到指定obj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key">键名</param>
    /// <returns></returns>
    private static JContainer? FindKey(this JArray obj, string key)
    {
        foreach (var item in obj)
        {
            if (item is JObject obj1)
            {
                var data = FindKey(obj1, key);
                if (data != null)
                    return data.Parent;
            }
        }

        return null;
    }

    /// <summary>
    /// 禁用Mod
    /// </summary>
    /// <param name="mod"></param>
    public static void Disable(this ModObj mod)
    {
        if (mod.Disable)
            return;

        var file = new FileInfo(mod.Local);
        mod.Disable = true;
        mod.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name
            .Replace(".jar", ".disable")}");
        File.Move(file.FullName, mod.Local);
    }

    /// <summary>
    /// 启用Mod
    /// </summary>
    /// <param name="mod"></param>
    public static void Enable(this ModObj mod)
    {
        if (!mod.Disable)
            return;

        var file = new FileInfo(mod.Local);
        mod.Disable = false;
        mod.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name
            .Replace(".disable", ".jar")}");
        File.Move(file.FullName, mod.Local);
    }

    /// <summary>
    /// 删除Mod
    /// </summary>
    /// <param name="mod"></param>
    public static void Delete(this ModObj mod)
    {
        File.Delete(mod.Local);
    }

    /// <summary>
    /// 导入Mod
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    public static async Task<bool> AddMods(this GameSettingObj obj, List<string> file)
    {
        if (file.Count == 0)
            return false;
        string path = obj.GetModsPath();
        bool ok = true;
        await Parallel.ForEachAsync(file, async (item, cancel) =>
        {
            var info = new FileInfo(item);
            if (!info.Exists)
                return;

            var info1 = new FileInfo(Path.GetFullPath(path + "/" + info.Name));
            if (info1.Exists)
            {
                info1.Delete();
            }

            try
            {
                File.Copy(info.FullName, info1.FullName);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.GetName("Core.Game.Error3"), e);
                ok = false;
                return;
            }
        });
        if (!ok)
            return false;

        return true;
    }

    /// <summary>
    /// 作者分割
    /// </summary>
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

    /// <summary>
    /// 作者分割
    /// </summary>
    private static List<string> ToStringList(this JArray array)
    {
        List<string> list = new();
        foreach (var item in array)
        {
            if (item is JObject obj && obj.ContainsKey("name"))
            {
                list.Add(item["name"]!.ToString());
            }
            else
            {
                list.Add(item.ToString());
            }
        }

        return list;
    }

    /// <summary>
    /// 作者分割
    /// </summary>
    private static List<string> ToStringList(this JObject array)
    {
        List<string> list = new();
        foreach (var item in array)
        {
            list.Add(item.Key.ToString());
        }

        return list;
    }

    /// <summary>
    /// 添加Mod信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="info"></param>
    public static void AddModInfo(this GameSettingObj obj, ModInfoObj info)
    {
        if (obj.Mods.ContainsKey(info.ModId))
        {
            obj.Mods[info.ModId] = info;
        }
        else
        {
            obj.Mods.Add(info.ModId, info);
        }
        obj.SaveModInfo();
    }
}
