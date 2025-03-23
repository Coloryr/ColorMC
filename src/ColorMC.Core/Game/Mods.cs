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
/// 游戏模组相关操作
/// </summary>
public static class Mods
{
    private static readonly char[] s_separator = ['\n'];

    public static async Task<List<ModObj>> GetModFastAsync(this GameSettingObj obj)
    {
        var dir = obj.GetModsPath();
        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return [];
        }

        var list = new List<ModObj>();
        var files = info.GetFiles();
#if false
        await Parallel.ForEachAsync(files, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 1
        }, async (item, cancel) =>
#else
        await Parallel.ForEachAsync(files, async (item, cancel) =>
#endif
        {
            if (item.Extension is not (Names.NameZipExt or Names.NameJarExt or Names.NameDisableExt or Names.NameDisabledExt))
            {
                using var filestream = PathHelper.OpenRead(item.FullName)!;
                var sha1 = await HashHelper.GenSha1Async(filestream);
                filestream.Seek(0, SeekOrigin.Begin);

                list.Add(new()
                {
                    Local = item.FullName,
                    Sha1 = sha1
                });
            }
        });

        return list;
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="sha256">是否获取SHA256</param>
    /// <returns>Mod列表</returns>
    public static async Task<List<ModObj>> GetModsAsync(this GameSettingObj obj, bool sha256 = false)
    {
        var dir = obj.GetModsPath();
        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return [];
        }
        var list = new ConcurrentBag<ModObj>();
        var files = info.GetFiles();

        //多线程同时检查
#if false
        await Parallel.ForEachAsync(files, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 1
        }, async (item, cancel) =>
#else
        await Parallel.ForEachAsync(files, async (item, cancel) =>
#endif
        {
            var mod = await ReadMod(item, sha256);
            if (mod != null)
            {
                mod.Game = obj;
                list.Add(mod);
            }
        });

        //排序
        var list1 = list.ToList();
        list1.Sort(ModComparer.Instance);

        return list1;
    }

    /// <summary>
    /// 读取模组
    /// </summary>
    /// <param name="item">文件</param>
    /// <param name="sha256">是否获取sha256</param>
    /// <returns>模组信息</returns>
    private static async Task<ModObj?> ReadMod(FileInfo item, bool sha256)
    {
        if (item.Extension is not (Names.NameZipExt or Names.NameJarExt or Names.NameDisableExt or Names.NameDisabledExt))
        {
            return null;
        }

        var sha1 = "";
        try
        {
            using var filestream = PathHelper.OpenRead(item.FullName)!;
            sha1 = HashHelper.GenSha1(filestream);
            filestream.Seek(0, SeekOrigin.Begin);

            using var zFile = new ZipFile(filestream);
            var mod = await ReadModAsync(zFile);
            if (mod != null)
            {
                mod.Local = item.FullName;
                mod.Disable = item.Extension is Names.NameDisableExt or Names.NameDisabledExt;
                mod.Sha1 = sha1;
                mod.Name ??= "";
                mod.ModId ??= "";
                if (sha256)
                {
                    filestream.Seek(0, SeekOrigin.Begin);
                    mod.Sha256 = HashHelper.GenSha256(filestream);
                }

                return mod;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error1"), e);
        }
        return new()
        {
            ModId = "",
            Name = "",
            Local = item.FullName,
            Disable = item.Extension is Names.NameDisableExt or Names.NameDisabledExt,
            ReadFail = true,
            Sha1 = sha1
        };
    }

    /// <summary>
    /// 根据模组在线信息读模组
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="mod">模组在线信息</param>
    /// <returns>模组信息</returns>
    public static async Task<ModObj?> ReadMod(this GameSettingObj obj, ModInfoObj? mod)
    {
        if (mod == null)
        {
            return null;
        }
        var file = Path.Combine(obj.GetModsPath(), mod.File);
        if (File.Exists(file))
        {
            var info = new FileInfo(file);
            var mod1 = await ReadMod(info, false);
            if (mod1 != null)
            {
                mod1.Game = obj;
            }
            return mod1;
        }

        return null;
    }

    /// <summary>
    /// 禁用模组
    /// </summary>
    /// <param name="mod">游戏模组</param>
    public static void Disable(this ModObj mod)
    {
        if (mod.Disable || !File.Exists(mod.Local))
        {
            return;
        }

        var file = new FileInfo(mod.Local);
        mod.Disable = true;
        mod.Local = Path.ChangeExtension(mod.Local, Names.NameDisableExt);
        PathHelper.MoveFile(file.FullName, mod.Local);

        var info = mod.Game.Mods.Values.FirstOrDefault(item => item.Sha1 == mod.Sha1);
        if (info != null)
        {
            info.File = Path.ChangeExtension(info.File, Names.NameDisableExt);
            mod.Game.SaveModInfo();
        }
    }

    /// <summary>
    /// 启用模组
    /// </summary>
    /// <param name="mod">游戏模组</param>
    public static void Enable(this ModObj mod)
    {
        if (!mod.Disable || !File.Exists(mod.Local))
        {
            return;
        }

        var file = new FileInfo(mod.Local);
        mod.Disable = false;
        mod.Local = Path.ChangeExtension(mod.Local, Names.NameJarExt);
        PathHelper.MoveFile(file.FullName, mod.Local);

        var info = mod.Game.Mods.Values.FirstOrDefault(item => item.Sha1 == mod.Sha1);
        if (info != null)
        {
            info.File = Path.ChangeExtension(info.File, Names.NameJarExt);
            mod.Game.SaveModInfo();
        }
    }

    /// <summary>
    /// 删除模组
    /// </summary>
    /// <param name="mod">游戏模组</param>
    public static void Delete(this ModObj mod)
    {
        PathHelper.Delete(mod.Local);
    }

    /// <summary>
    /// 导入模组
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
            var local = Path.Combine(path, Path.GetFileName(item));

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
    /// 添加模组信息
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
    /// 检查模组数量
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>模组数量</returns>
    public static int ReadModCountFast(this GameSettingObj obj)
    {
        string dir = obj.GetModsPath();

        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return 0;
        }
        var files = info.GetFiles();
        int a = 0;
        foreach (var item in files)
        {
            if (item.Name.EndsWith(Names.NameJarExt))
            {
                a++;
            }
        }

        return a;
    }

    /// <summary>
    /// 获取JarInJar列表
    /// </summary>
    /// <param name="obj">游戏Mod</param>
    /// <param name="zFile">Mod压缩包</param>
    private static async Task CheckJarInJarAsync(ModObj obj, ZipFile zFile)
    {
        obj.InJar ??= [];
        foreach (ZipEntry item3 in zFile)
        {
            if (item3.Name.EndsWith(Names.NameJarExt) && item3.Name.StartsWith("META-INF/jarjar/"))
            {
                using var filestream = zFile.GetInputStream(item3);
                using var stream2 = new MemoryStream();
                await filestream.CopyToAsync(stream2);
                stream2.Seek(0, SeekOrigin.Begin);
                using var zFile1 = new ZipFile(stream2);
                var inmod = await ReadModAsync(zFile1);
                if (inmod != null)
                {
                    obj.InJar.Add(inmod);
                }
            }
        }
    }

    /// <summary>
    /// 读取一个Mod文件
    /// </summary>
    /// <param name="zFile">Mod压缩包</param>
    /// <returns>游戏Mod</returns>
    private static async Task<ModObj?> ReadModAsync(ZipFile zFile)
    {
        bool istest = false;
        var mod = new ModObj()
        {
            Loaders = [],
            Dependants = [],
            Author = []
        };

        //forge 1.13以下
        var item1 = zFile.GetEntry("mcmod.info");
        if (item1 != null)
        {
            try
            {
                using var stream1 = zFile.GetInputStream(item1);
                using var stream2 = new MemoryStream();
                await stream1.CopyToAsync(stream2);
                var data = Encoding.UTF8.GetString(stream2.ToArray());
                var obj1 = Parse(data);
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
                    mod.Name = obj3["name"]!.ToString();
                    mod.ModId = obj3["modid"]!.ToString();
                    mod.Description = obj3["description"]?.ToString();
                    mod.Version = obj3["version"]?.ToString();
                    mod.Url = obj3["url"]?.ToString();
                    mod.Loaders.Add(Loaders.Forge);
                    mod.Side = SideType.None; //无法判断sideonly

                    if (obj3.TryGetValue("authorList", out var value))
                    {
                        var list1 = value.ToObject<List<string>>()!;
                        foreach (var item in list1)
                        {
                            mod.Author.Add(item);
                        }
                    }

                    if (obj3.TryGetValue("dependants", out value))
                    {
                        var list1 = value.ToObject<List<string>>()!;
                        foreach (var item in list1)
                        {
                            mod.Dependants.Add(item);
                        }
                    }

                    if (obj3.TryGetValue("dependencies", out value))
                    {
                        var list1 = value.ToObject<List<string>>()!;
                        foreach (var item in list1)
                        {
                            mod.Dependants.Add(item);
                        }
                    }

                    if (obj3.TryGetValue("requiredMods", out value))
                    {
                        var list1 = value.ToObject<List<string>>()!;
                        foreach (var item in list1)
                        {
                            mod.Dependants.Add(item);
                        }
                    }

                    istest = true;
                }
            }
            catch
            {

            }
        }

        //forge 1.13及以上
        bool neoforge = false;
        item1 = zFile.GetEntry("META-INF/mods.toml");
        if (item1 == null)
        {
            //neoforge 1.20.5及以上
            item1 = zFile.GetEntry("META-INF/neoforge.mods.toml");
            neoforge = true;
            item1 ??= zFile.GetEntry("neoforge.mods.toml");
        }
        if (item1 != null)
        {
            mod.Loaders.Add(neoforge ? Loaders.NeoForge : Loaders.Forge);
            if (!istest)
            {
                try
                {
                    using var stream1 = zFile.GetInputStream(item1);
                    using var stream = new MemoryStream();
                    await stream1.CopyToAsync(stream);
                    var model = Toml.Parse(stream.ToArray()).ToModel();
                    TomlTable? model2 = null;
                    if (model["mods"] is TomlArray array)
                    {
                        model2 = array.FirstOrDefault() as TomlTable;
                    }
                    else if (model["mods"] is TomlTableArray model1)
                    {
                        model2 = model1[0];
                    }
                    if (model2 == null)
                    {
                        return null;
                    }
                    mod.ModId = model2["modId"].ToString()!;
                    mod.Name = model2["displayName"].ToString()!;
                    if (model2.TryGetValue("description", out var item2))
                    {
                        mod.Description = item2 as string;
                    }
                    if (model2.TryGetValue("version", out item2))
                    {
                        mod.Version = item2 as string;
                    }
                    if (model2.TryGetValue("authorList", out item2))
                    {
                        var list = (item2 as string)!.ToStringList();
                        foreach (var item3 in list)
                        {
                            mod.Author.Add(item3);
                        }
                    }
                    //forge 1.20
                    else if (model2.TryGetValue("authors", out item2))
                    {
                        var list = (item2 as string)!.ToStringList();
                        foreach (var item3 in list)
                        {
                            mod.Author.Add(item3);
                        }
                    }
                    if (model2.TryGetValue("displayURL", out item2))
                    {
                        mod.Url = item2 as string;
                    }

                    //依赖项
                    if (model.TryGetValue("dependencies", out var model3) && model3 is TomlTable model4
                        && model4.FirstOrDefault().Value is TomlTableArray model5)
                    {
                        foreach (var item3 in model5)
                        {
                            if (item3.TryGetValue("modId", out item2))
                            {
                                var modid = item2.ToString()!;
                                if (modid == "minecraft" && item3.TryGetValue("side", out var item4))
                                {
                                    var temp = item4.ToString()!.ToLower();
                                    if (temp == "both")
                                    {
                                        mod.Side = SideType.Both;
                                    }
                                    else if (temp == "client")
                                    {
                                        mod.Side = SideType.Client;
                                    }
                                    else if (temp == "server")
                                    {
                                        mod.Side = SideType.Server;
                                    }
                                }
                                else
                                {
                                    if (item3.TryGetValue("mandatory", out item4)
                                        && item4?.ToString()?.ToLower() == "true")
                                    {
                                        mod.Dependants.Add(modid);
                                        continue;
                                    }
                                    if (item3.TryGetValue("type", out item4)
                                        && item4?.ToString()?.ToLower() == "required")
                                    {
                                        mod.Dependants.Add(modid);
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    await CheckJarInJarAsync(mod, zFile);
                    istest = true;
                }
                catch
                {

                }
            }
        }

        //fabric
        item1 = zFile.GetEntry("fabric.mod.json");
        if (item1 != null)
        {
            mod.Loaders.Add(Loaders.Fabric);
            if (!istest)
            {
                try
                {
                    using var stream1 = zFile.GetInputStream(item1);
                    using var stream = new MemoryStream();
                    await stream1.CopyToAsync(stream);
                    var data = Encoding.UTF8.GetString(stream.ToArray());
                    var obj1 = JObject.Parse(data);
                    mod.ModId = obj1["id"]!.ToString();
                    mod.Name = obj1["name"]?.ToString() ?? mod.ModId;
                    mod.Description = obj1["description"]?.ToString();
                    mod.Version = obj1["version"]?.ToString();
                    mod.Url = obj1["contact"]?["homepage"]?.ToString();

                    var side = obj1["environment"]?.ToString().ToLower();
                    if (side == null)
                    {
                        mod.Side = SideType.None;
                    }
                    else if (side == "*")
                    {
                        mod.Side = SideType.Both;
                    }
                    else if (side == "client")
                    {
                        mod.Side = SideType.Client;
                    }
                    else if (side == "server")
                    {
                        mod.Side = SideType.Server;
                    }

                    if (obj1.TryGetValue("authors", out var list) && list is JArray array)
                    {
                        foreach (var item in array.ToStringList())
                        {
                            mod.Dependants.Add(item);
                        }
                    }

                    if (obj1.TryGetValue("depends", out var list1) && list1 is JObject array2)
                    {
                        foreach (var item3 in array2)
                        {
                            mod.Dependants.Add(item3.Key);
                        }
                    }

                    await CheckJarInJarAsync(mod, zFile);

                    istest = true;
                }
                catch
                {

                }
            }
        }

        //quilt
        item1 = zFile.GetEntry("quilt.mod.json");
        if (item1 != null)
        {
            mod.Loaders.Add(Loaders.Quilt);
            if (!istest)
            {
                try
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
                    mod.ModId = obj4["id"]!.ToString();
                    mod.Name = obj4["metadata"]!["name"]!.ToString();
                    mod.Description = obj4["metadata"]?["description"]?.ToString();
                    mod.Version = obj4["version"]?.ToString();
                    mod.Url = obj4["contact"]?["homepage"]?.ToString();
                    if (obj4["metadata"]?["contributors"] is JObject array)
                    {
                        var list = array.ToStringList();
                        foreach (var item in list)
                        {
                            mod.Author.Add(item);
                        }
                    }

                    if (obj4["depends"] is JArray obj5)
                    {
                        foreach (var item3 in obj5)
                        {
                            if (item3?["id"]?.ToString() is string str)
                            {
                                mod.Dependants.Add(str);
                            }
                        }
                    }

                    await CheckJarInJarAsync(mod, zFile);
                    istest = true;
                }
                catch
                {

                }
            }
        }

        //core mod
        item1 = zFile.GetEntry("META-INF/services/cpw.mods.modlauncher.api.ITransformationService");
        var item5 = zFile.GetEntry("META-INF/services/net.minecraftforge.forgespi.language.IModLanguageProvider");
        var item7 = zFile.GetEntry("META-INF/services/net.neoforged.neoforgespi.language.IModLanguageLoader");
        var item6 = zFile.GetEntry("META-INF/MANIFEST.MF");
        if (item6 != null)
        {
            using var stream12 = zFile.GetInputStream(item6);
            using var reader = new StreamReader(stream12);
            var con = Options.ReadOptions(reader.ReadToEnd());
            if (item7 != null)
            {
                mod.CoreMod = true;
                mod.Loaders.Add(Loaders.NeoForge);
                if (!istest)
                {
                    if (!con.TryGetValue("Automatic-Module-Name", out string? name)
                        && !con.TryGetValue("Specification-Title", out name)
                        && !con.TryGetValue("Implementation-Title", out name)
                        && !con.TryGetValue("Automatic-Module-Name", out name))
                    {
                        name = "";
                    }
                    name = name.Trim();

                    if (con.TryGetValue("Implementation-Version", out string? version))
                    {
                        mod.Version = version;
                    }

                    mod.Name = name;
                    mod.ModId = name.ToLower();

                    await CheckJarInJarAsync(mod, zFile);

                    istest = true;
                }
            }
            else if (item1 != null || item5 != null)
            {
                mod.CoreMod = true;
                mod.Loaders.Add(Loaders.Forge);
                if (!istest)
                {
                    if (!con.TryGetValue("Specification-Title", out string? name)
                        && !con.TryGetValue("Implementation-Title", out name)
                        && !con.TryGetValue("Automatic-Module-Name", out name))
                    {
                        name = "";
                    }
                    name = name.Trim();
                    mod.Name = name;
                    mod.ModId = name.ToLower();

                    await CheckJarInJarAsync(mod, zFile);

                    istest = true;
                }
            }
            else if (con.TryGetValue("FMLCorePlugin", out string? fml))
            {
                mod.CoreMod = true;
                mod.Loaders.Add(Loaders.Forge);
                if (!istest)
                {
                    fml = fml.Trim();
                    mod.Name = fml;
                    mod.ModId = fml.ToLower();

                    istest = true;
                }
            }
        }


        item1 = zFile.GetEntry("META-INF/fml_cache_annotation.json");
        //forge coremod
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream2 = new MemoryStream();
            await stream1.CopyToAsync(stream2);
            var data = Encoding.UTF8.GetString(stream2.ToArray());
            var obj1 = JObject.Parse(data);
            var obj2 = FindKey(obj1, "acceptedMinecraftVersions");
            if (obj2 != null)
            {
                mod.CoreMod = true;
                mod.Loaders.Add(Loaders.Forge);
                if (!istest)
                {
                    if (obj2["modId"]?["value"]?.ToString() is { } str)
                    {
                        mod.ModId = str;
                    }
                    else if (obj2["modid"]?["value"]?.ToString() is { } str1)
                    {
                        mod.ModId = str1;
                    }

                    if (obj2["names"]?["value"]?.ToString() is { } str2)
                    {
                        mod.Name = str2;
                    }
                    else if (obj2["name"]?["value"]?.ToString() is { } str3)
                    {
                        mod.Name = str3;
                    }

                    if (obj2["version"]?["value"]?.ToString() is { } str4)
                    {
                        mod.Version = str4;
                    }

                    if (obj2["dependencies"]?["value"]?.ToString() is { } str5)
                    {
                        mod.Dependants.Add(str5);
                    }

                    istest = true;
                }
            }
        }

        if (!istest)
        {
            //使用jarjar的内容
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
                    if (inmod != null && !string.IsNullOrWhiteSpace(inmod.Name)
                         && !string.IsNullOrWhiteSpace(inmod.ModId) && !inmod.CoreMod)
                    {
                        return inmod;
                    }
                }
            }
        }

        if (istest)
        {
            return mod;
        }

        return null;
    }

    /// <summary>
    /// 解析数据
    /// </summary>
    /// <param name="data">Json数据</param>
    /// <returns>数据</returns>
    private static JToken Parse(string data)
    {
        try
        {
            return JToken.Parse(data);
        }
        catch (Exception)
        {
            var lines = data.Split(s_separator, StringSplitOptions.None);
            var token = new JObject();
            foreach (var line in lines)
            {
                try
                {
                    JToken parsedLine = JToken.Parse("{" + line + "}");
                    token.Add(parsedLine.First);
                }
                catch
                {

                }
            }

            return new JArray() { token };
        }
    }

    /// <summary>
    /// 作者分割
    /// </summary>
    /// <param name="obj">作者名</param>
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
    /// <param name="array">数据</param>
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
    /// <param name="array">数据</param>
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
    /// 找到指定数据
    /// </summary>
    /// <param name="obj">数据</param>
    /// <param name="key">键名</param>
    /// <returns>数据</returns>
    private static JToken? FindKey(this JObject obj, string key)
    {
        foreach (var item in obj)
        {
            if (item.Key == key)
            {
                return obj;
            }

            if (item.Value is JObject obj1
                && FindKey(obj1, key) is { } obj2)
            {
                return obj2;
            }
            else if (item.Value is JArray arry
                && FindKey(arry, key) is { } obj3)
            {
                return obj3;
            }
        }

        return null;
    }

    /// <summary>
    /// 找到指定数据
    /// </summary>
    /// <param name="obj">数据</param>
    /// <param name="key">键名</param>
    /// <returns>数据</returns>
    private static JToken? FindKey(this JArray obj, string key)
    {
        foreach (var item in obj)
        {
            if (item is JObject obj1)
            {
                var data = FindKey(obj1, key);
                if (data != null)
                {
                    return data;
                }
            }
        }

        return null;
    }
}
