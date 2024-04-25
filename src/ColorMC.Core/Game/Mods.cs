using System.Collections.Concurrent;
using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
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
    private static async Task<ModObj?> ReadModAsync(ZipFile zFile)
    {
        //forge 1.13以下
        var item1 = zFile.GetEntry("mcmod.info");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream2 = new MemoryStream();
            await stream1.CopyToAsync(stream2);
            var data = Encoding.UTF8.GetString(stream2.ToArray());
            var obj1 = JToken.Parse(data);
            JObject obj3;
            if (obj1 is JArray array)
            {
                obj3 = (array[0] as JObject)!;
            }
            else
            {
                obj3 = (obj1["modList"]![0] as JObject)!;
            }
            if (obj3 != null)
            {
                var obj4 = obj3.ToObject<ModObj>()!;
                obj4.name ??= "";
                obj4.modid ??= "";
                obj4.V2 = false;
                obj4.Loader = Loaders.Forge;
                return obj4;
            }
        }

        //forge coremod
        item1 = zFile.GetEntry("META-INF/fml_cache_annotation.json");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream2 = new MemoryStream();
            await stream1.CopyToAsync(stream2);
            var data = Encoding.UTF8.GetString(stream2.ToArray());
            var obj1 = JObject.Parse(data);
            var obj2 = FindKey(obj1, "acceptedMinecraftVersions");
            if (obj2 == null)
            {
                return null;
            }

            var obj3 = new ModObj
            {
                V2 = true,
                Loader = Loaders.Forge,
                modid = obj2["modId"]?["value"]?.ToString() ?? "",
                name = obj2["names"]?["value"]?.ToString() ?? "",
                version = obj2["version"]?["value"]?.ToString(),
                requiredMods = [obj2["dependencies"]?["value"]?.ToString() ?? ""],
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
            {
                return null;
            }
            var model2 = model1[0];
            if (model2 == null)
            {
                return null;
            }
            var obj3 = new ModObj
            {
                V2 = true,
                Loader = Loaders.Forge,
            };
            if (model2.TryGetValue("modId", out object item2))
            {
                obj3.modid = item2 as string ?? "";
            }
            if (model2.TryGetValue("displayName", out item2))
            {
                obj3.name = item2 as string ?? "";
            }
            if (model2.TryGetValue("modId", out item2))
            {
                obj3.modid = item2 as string ?? "";
            }
            if (model2.TryGetValue("description", out item2))
            {
                obj3.description = item2 as string;
            }
            if (model2.TryGetValue("version", out item2))
            {
                obj3.version = item2 as string;
            }
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

            //依赖项
            if (model.TryGetValue("dependencies", out var model3) && model3 is TomlTable model4)
            {
                obj3.requiredMods = [];
                if (model4.FirstOrDefault().Value is TomlTableArray model5)
                {
                    foreach (var item3 in model5)
                    {
                        if (item3.TryGetValue("modId", out item2)
                        && item3.TryGetValue("mandatory", out var item4)
                        && item4?.ToString()?.ToLower() == "true"
                        && item2 is string str)
                        {
                            obj3.requiredMods.Add(str);
                        }
                    }
                }
            }

            obj3.InJar = [];
            foreach (ZipEntry item3 in zFile)
            {
                if (item3.Name.EndsWith(".jar") && item3.Name.StartsWith("META-INF/jarjar/"))
                {
                    using var filestream = zFile.GetInputStream(item3);
                    using var stream2 = new MemoryStream();
                    await filestream.CopyToAsync(stream2);
                    stream2.Seek(0, SeekOrigin.Begin);
                    using var zFile1 = new ZipFile(stream2);
                    var inmod = await ReadModAsync(zFile1);
                    if (inmod != null)
                    {
                        obj3.InJar.Add(inmod);
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
                modid = obj1["id"]?.ToString() ?? "",
                name = obj1["name"]?.ToString() ?? "",
                description = obj1["description"]?.ToString(),
                version = obj1["version"]?.ToString(),
                authorList = (obj1["authors"] as JArray)?.ToStringList(),
                url = obj1["contact"]?["homepage"]?.ToString(),
            };

            obj3.name ??= "";

            if (obj1.ContainsKey("depends"))
            {
                obj3.requiredMods = [];
                if (obj1["depends"] is JObject obj4)
                {
                    foreach (var item3 in obj4)
                    {
                        obj3.requiredMods.Add(item3.Key);
                    }
                }
            }

            //JarInJar
            obj3.InJar = [];

            foreach (ZipEntry item2 in zFile)
            {
                if (item2.Name.EndsWith(".jar") && item2.Name.StartsWith("META-INF/jars/"))
                {
                    using var filestream = zFile.GetInputStream(item2);
                    using var stream2 = new MemoryStream();
                    await filestream.CopyToAsync(stream2);
                    stream2.Seek(0, SeekOrigin.Begin);
                    using var zFile1 = new ZipFile(stream2);
                    var inmod = await ReadModAsync(zFile1);
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
            if (obj1?["quilt_loader"] is not JObject obj4)
            {
                return null;
            }
            var obj3 = new ModObj
            {
                Loader = Loaders.Quilt,
                V2 = true,
                modid = obj4["id"]?.ToString() ?? "",
                name = obj4["metadata"]?["name"]?.ToString() ?? "",
                description = obj4["metadata"]?["description"]?.ToString(),
                version = obj4["version"]?.ToString(),
                authorList = (obj4["metadata"]?["contributors"] as JObject)?.ToStringList(),
                url = obj4["contact"]?["homepage"]?.ToString(),
            };

            obj3.name ??= "";

            obj3.requiredMods = [];
            if (obj4["depends"] is JArray obj5)
            {
                foreach (var item3 in obj5)
                {
                    if (item3?["id"]?.ToString() is string str)
                    {
                        obj3.requiredMods.Add(str);
                    }
                }
            }

            //JarInJar
            obj3.InJar = [];

            foreach (ZipEntry item2 in zFile)
            {
                if (item2.Name.EndsWith(".jar") && item2.Name.StartsWith("META-INF/jars/"))
                {
                    using var filestream = zFile.GetInputStream(item2);
                    using var zFile1 = new ZipFile(filestream);
                    var inmod = await ReadModAsync(zFile1);
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
    public static async Task<List<ModObj>> GetModsAsync(this GameSettingObj obj, bool sha256)
    {
        var list = new ConcurrentBag<ModObj>();
        var dir = obj.GetModsPath();

        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return [.. list];
        }
        var files = info.GetFiles();

        //多线程同时检查
        //await Parallel.ForEachAsync(files, new ParallelOptions()
        //{
        //    MaxDegreeOfParallelism = 1
        //}, async (item, cancel) =>
        await Parallel.ForEachAsync(files, async (item, cancel) =>
        {
            if (item.Extension is not (".zip" or ".jar" or ".disable"))
            {
                return;
            }

            var sha1 = "";
            bool add = false;
            try
            {
                using var filestream = PathHelper.OpenRead(item.FullName)!;
                sha1 = HashHelper.GenSha1(filestream);
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

                using var zFile = new ZipFile(filestream);
                var mod = await ReadModAsync(zFile);
                if (mod != null)
                {
                    mod.Local = Path.GetFullPath(item.FullName);
                    mod.Disable = item.Extension is ".disable";
                    mod.Sha1 = sha1;
                    mod.Game = obj;
                    list.Add(mod);
                    add = true;
                    if (sha256)
                    {
                        filestream.Seek(0, SeekOrigin.Begin);
                        mod.Sha256 = HashHelper.GenSha256(filestream);
                    }
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

        //排序
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
        PathHelper.MoveFile(file.FullName, mod.Local);
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
        PathHelper.MoveFile(file.FullName, mod.Local);
    }

    /// <summary>
    /// 删除Mod
    /// </summary>
    /// <param name="mod">游戏Mod</param>
    public static void Delete(this ModObj mod)
    {
        PathHelper.Delete(mod.Local);
    }

    /// <summary>
    /// 导入Mod
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns>是否成功导入</returns>
    public static async Task<bool> AddModsAsync(this GameSettingObj obj, List<string> file)
    {
        if (file.Count == 0)
        {
            return false;
        }
        var path = obj.GetModsPath();
        var ok = true;
        await Task.Run(() => Parallel.ForEach(file, async (item) =>
        {
            var name = Path.GetFileName(item);
            var local = Path.GetFullPath(path + "/" + name);

            await Task.Run(() =>
            {
                try
                {
                    PathHelper.CopyFile(item, local);
                }
                catch (Exception e)
                {
                    Logs.Error(LanguageHelper.Get("Core.Game.Error3"), e);
                    ok = false;
                }
            });
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
        if (!obj.Mods.TryAdd(info.ModId, info))
        {
            obj.Mods[info.ModId] = info;
        }

        obj.SaveModInfo();
    }

    /// <summary>
    /// 检查有无mod存在
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool GetModeFast(this GameSettingObj obj)
    {
        string dir = obj.GetModsPath();

        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return false;
        }
        var files = info.GetFiles();
        foreach (var item in files)
        {
            if (item.Name.EndsWith(".jar"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 作者分割
    /// </summary>
    /// <returns>整理好的作者名</returns>
    private static List<string> ToStringList(this string obj)
    {
        var list = new List<string>();
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
        var list = new List<string>();
        foreach (var item in array)
        {
            if (item is JObject obj && obj.ContainsKey("name"))
            {
                list.Add(item["name"]!.ToString());
            }
            else
            {
                list.Add(item!.ToString());
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
        var list = new List<string>();
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
    private static JToken? FindKey(this JObject obj, string key)
    {
        foreach (var item in obj)
        {
            if (item.Key == key)
            {
                return item.Value!.Root;
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
