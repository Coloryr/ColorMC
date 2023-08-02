using ColorMC.Core.Helpers;
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

/// <summary>
/// 游戏Mod相关操作
/// </summary>
public static class Mods
{
    /// <summary>
    /// 读取一个Mod文件
    /// </summary>
    /// <param name="zFile">Mod压缩包</param>
    /// <returns>游戏Mod</returns>
    private static async Task<ModObj?> ReadMod(ZipFile zFile)
    {
        //forge 1.13以下
        var item1 = zFile.GetEntry("mcmod.info");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream = new MemoryStream();
            await stream1.CopyToAsync(stream);
            var data = Encoding.UTF8.GetString(stream.ToArray());
            if (data.StartsWith("{"))
            {
                var obj1 = JObject.Parse(data);
                var obj2 = obj1.GetValue("modList") as JArray;
                if (obj2?.Count > 0)
                {
                    var obj3 = obj2.First().ToObject<ModObj>()!;
                    obj3.name ??= "";
                    obj3.modid ??= "";
                    obj3.V2 = false;
                    obj3.Loader = Loaders.Forge;
                    return obj3;
                }
            }
            else if (data.StartsWith("["))
            {
                var obj1 = JArray.Parse(data);
                if (obj1?.Count > 0)
                {
                    var obj3 = obj1.First().ToObject<ModObj>()!;
                    obj3.name ??= "";
                    obj3.modid ??= "";
                    obj3.V2 = false;
                    obj3.Loader = Loaders.Forge;
                    return obj3;
                }
            }
        }

        //forge coremod
        item1 = zFile.GetEntry("META-INF/fml_cache_annotation.json");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream = new MemoryStream();
            await stream1.CopyToAsync(stream);
            var data = Encoding.UTF8.GetString(stream.ToArray());
            var obj1 = JObject.Parse(data);
            var obj2 = FindKey(obj1, "acceptedMinecraftVersions");
            if (obj2 == null)
                return null;

            ModObj obj3 = new()
            {
                V2 = true,
                Loader = Loaders.Forge,
                modid = obj2["modId"]?["value"]?.ToString(),
                name = obj2["names"]?["value"]?.ToString(),
                version = obj2["version"]?["value"]?.ToString(),
                requiredMods = new() { obj2["dependencies"]?["value"]?.ToString() },
                CoreMod = true
            };

            obj3.modid ??= "";
            obj3.name ??= obj3.modid;

            return obj3;
        }

        //forge 1.13及以上
        item1 = zFile.GetEntry("META-INF/mods.toml");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream = new MemoryStream();
            await stream1.CopyToAsync(stream);
            var model = Toml.Parse(stream.ToArray()).ToModel();
            if (model["mods"] is not TomlTableArray model1)
                return null;
            var model2 = model1[0];
            if (model2 == null)
                return null;
            ModObj obj3 = new()
            {
                V2 = true,
                Loader = Loaders.Forge,
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
            if (model2.TryGetValue("authorList", out item2))
            {
                obj3.authorList = (item2 as string)?.ToStringList();
            }
            //forge 1.20
            else if (model2.TryGetValue("authors", out item2))
            {
                obj3.authorList = (item2 as string)?.ToStringList();
            }
            if (model2.TryGetValue("displayURL", out item2))
            {
                obj3.url = item2 as string;
            }

            obj3.name ??= "";

            if (model["dependencies"] is TomlTable model3)
            {
                obj3.requiredMods = new();
                if (model3.FirstOrDefault().Value is TomlTableArray model4)
                {
                    foreach (var item3 in model4)
                    {
                        if (item3.TryGetValue("modId", out item2)
                        && item3.TryGetValue("mandatory", out var item4)
                        && item4?.ToString()?.ToLower() == "true")
                        {
                            obj3.requiredMods.Add(item2 as string);
                        }
                    }
                }
            }

            return obj3;
        }

        //fabric
        item1 = zFile.GetEntry("fabric.mod.json");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream = new MemoryStream();
            await stream1.CopyToAsync(stream);
            var data = Encoding.UTF8.GetString(stream.ToArray());
            var obj1 = JObject.Parse(data);
            var obj3 = new ModObj
            {
                Loader = Loaders.Fabric,
                V2 = true,
                modid = obj1["id"]?.ToString(),
                name = obj1["name"]?.ToString(),
                description = obj1["description"]?.ToString(),
                version = obj1["version"]?.ToString(),
                authorList = (obj1["authors"] as JArray)?.ToStringList(),
                url = obj1["contact"]?["homepage"]?.ToString(),
            };

            obj3.name ??= "";

            if (obj1.ContainsKey("depends"))
            {
                obj3.requiredMods = new();
                foreach (var item3 in (obj1["depends"] as JObject)!.Properties())
                {
                    obj3.requiredMods.Add(item3.Name);
                }
            }

            obj3.InJar = new();

            foreach (ZipEntry item2 in zFile)
            {
                if (item2.Name.EndsWith(".jar") && item2.Name.StartsWith("META-INF/jars/"))
                {
                    using var filestream = zFile.GetInputStream(item2);
                    using var stream2 = new MemoryStream();
                    await filestream.CopyToAsync(stream2);
                    stream2.Seek(0, SeekOrigin.Begin);
                    using ZipFile zFile1 = new(stream2);
                    var inmod = await ReadMod(zFile1);
                    if (inmod != null)
                    {
                        obj3.InJar.Add(inmod);
                    }
                }
            }

            return obj3;
        }

        //quilt
        item1 = zFile.GetEntry("quilt.mod.json");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream = new MemoryStream();
            await stream1.CopyToAsync(stream);
            var data = Encoding.UTF8.GetString(stream.ToArray());
            var obj1 = JObject.Parse(data);
            if (obj1["quilt_loader"] is not JObject obj2)
            {
                return null;
            }
            var obj3 = new ModObj
            {
                Loader = Loaders.Quilt,
                V2 = true,
                modid = obj2["id"]?.ToString(),
                name = obj2["metadata"]?["name"]?.ToString(),
                description = obj2["metadata"]?["description"]?.ToString(),
                version = obj2["version"]?.ToString(),
                authorList = (obj2["metadata"]?["contributors"] as JObject)?.ToStringList(),
                url = obj2["contact"]?["homepage"]?.ToString(),
            };

            obj3.name ??= "";

            if (obj2.ContainsKey("depends"))
            {
                obj3.requiredMods = new();
                foreach (var item3 in obj2["depends"]!)
                {
                    obj3.requiredMods.Add(item3["id"]?.ToString());
                }
            }

            obj3.InJar = new();

            foreach (ZipEntry item2 in zFile)
            {
                if (item2.Name.EndsWith(".jar") && item2.Name.StartsWith("META-INF/jars/"))
                {
                    using var filestream = zFile.GetInputStream(item2);
                    using ZipFile zFile1 = new(filestream);
                    var inmod = await ReadMod(zFile1);
                    if (inmod != null)
                    {
                        obj3.InJar.Add(inmod);
                    }
                }
            }

            return obj3;
        }

        return null;
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>Mod列表</returns>
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
        // await Parallel.ForEachAsync(files, new ParallelOptions()
        // {
        //     MaxDegreeOfParallelism = 1
        // }, async (item, cancel) =>
        await Parallel.ForEachAsync(files, async (item, cancel) =>
        {
            if (item.Extension is not (".zip" or ".jar" or ".disable"))
            {
                return;
            }

            string sha1 = "";
            bool add = false;
            try
            {
                using var filestream = File.OpenRead(item.FullName);
                sha1 = Funtions.GenSha1(filestream);
                filestream.Seek(0, SeekOrigin.Begin);

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
                    add = true;
                    return;
                }

                using ZipFile zFile = new(filestream);
                var mod = await ReadMod(zFile);
                if (mod != null)
                {
                    mod.Local = Path.GetFullPath(item.FullName);
                    mod.Disable = item.Extension is ".disable";
                    mod.Sha1 = sha1;
                    mod.Game = obj;
                    list.Add(mod);
                    add = true;
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Game.Error1"), e);
            }
            finally
            {
                if (!add)
                {
                    list.Add(new()
                    {
                        name = "",
                        Local = Path.GetFullPath(item.FullName),
                        Disable = item.Extension is ".disable",
                        ReadFail = true,
                        Sha1 = sha1,
                        Game = obj
                    });
                }
            }
        });

        var list1 = list.ToList();
        list1.Sort(ModComparer.Instance);

        return list1;
    }

    /// <summary>
    /// 禁用Mod
    /// </summary>
    /// <param name="mod">游戏Mod</param>
    public static void Disable(this ModObj mod)
    {
        if (mod.Disable)
        {
            return;
        }

        var file = new FileInfo(mod.Local);
        mod.Disable = true;
        mod.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name
            .Replace(".jar", ".disable")}");
        File.Move(file.FullName, mod.Local);
    }

    /// <summary>
    /// 启用Mod
    /// </summary>
    /// <param name="mod">游戏Mod</param>
    public static void Enable(this ModObj mod)
    {
        if (!mod.Disable)
        {
            return;
        }

        var file = new FileInfo(mod.Local);
        mod.Disable = false;
        mod.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name
            .Replace(".disable", ".jar")}");
        File.Move(file.FullName, mod.Local);
    }

    /// <summary>
    /// 删除Mod
    /// </summary>
    /// <param name="mod">游戏Mod</param>
    public static void Delete(this ModObj mod)
    {
        File.Delete(mod.Local);
    }

    /// <summary>
    /// 导入Mod
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns>是否成功导入</returns>
    public static async Task<bool> AddMods(this GameSettingObj obj, List<string> file)
    {
        if (file.Count == 0)
        {
            return false;
        }
        string path = obj.GetModsPath();
        bool ok = true;
        await Task.Run(() => Parallel.ForEach(file, (item) =>
        {
            var info = new FileInfo(item);
            if (!info.Exists)
            {
                return;
            }

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
                Logs.Error(LanguageHelper.Get("Core.Game.Error3"), e);
                ok = false;
                return;
            }
        }));
        if (!ok)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 添加Mod信息
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="info">信息</param>
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

    /// <summary>
    /// 作者分割
    /// </summary>
    /// <returns>整理好的作者名</returns>
    private static List<string> ToStringList(this string obj)
    {
        List<string> list = new();
        if (obj == null)
        {
            return list;
        }
        foreach (var item in obj.Split(","))
        {
            list.Add(item.Trim());
        }
        return list;
    }

    /// <summary>
    /// 作者分割
    /// </summary>
    /// <returns>整理好的作者名</returns>
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
    /// <returns>整理好的作者名</returns>
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
    /// 找到指定obj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key">键名</param>
    /// <returns>obj</returns>
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
    /// <returns>obj</returns>
    private static JContainer? FindKey(this JArray obj, string key)
    {
        foreach (var item in obj)
        {
            if (item is JObject obj1)
            {
                var data = FindKey(obj1, key);
                if (data != null)
                {
                    return data.Parent;
                }
            }
        }

        return null;
    }
}
