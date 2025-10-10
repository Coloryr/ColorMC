using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using SharpCompress.Archives.Zip;
using Tomlyn;
using Tomlyn.Model;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏模组相关操作
/// </summary>
public static class GameMods
{
    /// <summary>
    /// 获取模组列表，只获取文件名相关的
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>模组列表</returns>
    public static async Task<List<ModObj>> GetModFastAsync(this GameSettingObj obj)
    {
        var dir = obj.GetModsPath();
        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
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
            //之判断.jar为Mod
            if (item.Extension is Names.NameJarExt or Names.NameDisableExt or Names.NameDisabledExt)
            {
                using var filestream = PathHelper.OpenRead(item.FullName);
                if (filestream == null)
                {
                    return;
                }
                var sha1 = await HashHelper.GenSha1Async(filestream);

                list.Add(new ModObj
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
            var mod = await ReadModAsync(item, sha256);
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
    private static async Task<ModObj?> ReadModAsync(FileInfo item, bool sha256)
    {
        if (item.Extension is not (Names.NameJarExt or Names.NameDisableExt or Names.NameDisabledExt))
        {
            return null;
        }
        string sha1 = "";
        try
        {
            using var stream = PathHelper.OpenRead(item.FullName);
            if (stream == null)
            {
                return null;
            }
            sha1 = await HashHelper.GenSha1Async(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using var zFile = ZipArchive.Open(stream);
            var mod = await ReadModAsync(item.FullName, zFile);
            if (mod != null)
            {
                mod.Local = item.FullName;
                mod.Disable = item.Extension is Names.NameDisableExt or Names.NameDisabledExt; //其他启动器的禁用不一样
                mod.Name ??= "";
                mod.ModId ??= "";
                mod.Sha1 = sha1;
                if (!sha256)
                {
                    return mod;
                }

                stream.Seek(0, SeekOrigin.Begin);
                mod.Sha256 = await HashHelper.GenSha256Async(stream);

                return mod;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error1"), e);
        }
        return new ModObj
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
    public static async Task<ModObj?> ReadModAsync(this GameSettingObj obj, ModInfoObj? mod)
    {
        if (mod == null)
        {
            return null;
        }
        var file = Path.Combine(obj.GetModsPath(), mod.File);
        if (!File.Exists(file))
        {
            return null;
        }

        var info = new FileInfo(file);
        var mod1 = await ReadModAsync(info, false);
        if (mod1 != null)
        {
            mod1.Game = obj;
        }
        return mod1;
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
        if (info == null)
        {
            return;
        }

        info.File = Path.ChangeExtension(info.File, Names.NameDisableExt);
        mod.Game.SaveModInfo();
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
        if (info == null)
        {
            return;
        }

        info.File = Path.ChangeExtension(info.File, Names.NameJarExt);
        mod.Game.SaveModInfo();
    }

    /// <summary>
    /// 删除模组
    /// </summary>
    /// <param name="mod">游戏模组</param>
    public static Task DeleteAsync(this ModObj mod)
    {
        return PathHelper.MoveToTrashAsync(mod.Local);
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
        await Task.Run(() => Parallel.ForEach(file, item =>
        {
            var local = Path.Combine(path, Path.GetFileName(item));

            try
            {
                PathHelper.CopyFile(item, local);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Game.Error3"), e);
                ok = false;
            }
        }));
        return ok;
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
        //只需要读取能加载的mod数量
        return files.Count(item => item.Name.EndsWith(Names.NameJarExt));
    }

    /// <summary>
    /// 获取JarInJar列表
    /// </summary>
    /// <param name="obj">游戏Mod</param>
    /// <param name="zFile">Mod压缩包</param>
    private static async Task CheckJarInJarAsync(ModObj obj, ZipArchive zFile)
    {
        obj.InJar ??= [];
        foreach (var item3 in zFile.Entries)
        {
            if (item3.Key?.EndsWith(Names.NameJarExt) == false || item3.Key?.StartsWith(Names.NameModJarJarDir) == false)
            {
                continue;
            }

            using var stream = item3.OpenEntryStream();
            using var stream1 = new MemoryStream();
            await stream.CopyToAsync(stream1);
            using var zFile1 = ZipArchive.Open(stream1);
            var inmod = await ReadModAsync(obj.Local, zFile1);
            if (inmod != null)
            {
                obj.InJar.Add(inmod);
            }
        }
    }

    /// <summary>
    /// 读取一个Mod文件
    /// </summary>
    /// <param name="path">Mod路径</param>
    /// <param name="zFile">Mod压缩包</param>
    /// <returns>游戏Mod</returns>
    private static async Task<ModObj?> ReadModAsync(string path, ZipArchive zFile)
    {
        bool istest = false;
        var mod = new ModObj
        {
            Loaders = [],
            Dependants = [],
            Author = []
        };

        void ForgeOld(JsonObject obj)
        {
            mod.ModId = obj.GetString("modid") ?? "";
            mod.Name = obj.GetString("name") ?? mod.ModId;
            mod.Description = obj.GetString("description");
            mod.Version = obj.GetString("version");
            mod.Url = obj.GetString("url");
            mod.Loaders.Add(Loaders.Forge);
            mod.Side = SideType.None; //无法判断sideonly

            if (obj.GetArray("authorList") is { } list1)
            {
                foreach (var item in list1)
                {
                    if (item?.GetValue<string>() is { } str)
                    {
                        mod.Author.Add(str);
                    }
                }
            }

            if (obj.GetArray("dependants") is { } list2)
            {
                foreach (var item in list2)
                {
                    if (item?.GetValue<string>() is { } str)
                    {
                        mod.Dependants.Add(str);
                    }
                }
            }

            if (obj.GetArray("dependencies") is { } list3)
            {
                foreach (var item in list3)
                {
                    if (item?.GetValue<string>() is { } str)
                    {
                        mod.Dependants.Add(str);
                    }
                }
            }

            if (obj.GetArray("requiredMods") is { } list4)
            {
                foreach (var item in list4)
                {
                    if (item?.GetValue<string>() is { } str)
                    {
                        mod.Dependants.Add(str);
                    }
                }
            }

            istest = true;
        }

        async Task ForgeNewAsync(TomlTable model)
        {
            TomlTable? model2 = model["mods"] switch
            {
                TomlArray array => array.FirstOrDefault() as TomlTable,
                TomlTableArray model1 => model1[0],
                _ => null
            };
            if (model2 == null)
            {
                return;
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
                    if (!item3.TryGetValue("modId", out item2))
                    {
                        continue;
                    }

                    var modid = item2.ToString()!;
                    if (modid == "minecraft" && item3.TryGetValue("side", out var item4))
                    {
                        var temp = item4.ToString()!.ToLower();
                        mod.Side = temp switch
                        {
                            "both" => SideType.Both,
                            "client" => SideType.Client,
                            "server" => SideType.Server,
                            _ => mod.Side
                        };

                        continue;
                    }

                    if ((!item3.TryGetValue("mandatory", out item4) || item4.ToString()?.ToLower() != "true") &&
                            (!item3.TryGetValue("type", out item4) || item4.ToString()?.ToLower() != "required"))
                    {
                        continue;
                    }

                    mod.Dependants.Add(modid);
                }
            }

            await CheckJarInJarAsync(mod, zFile);
            istest = true;
        }

        async Task FabricAsync(JsonObject obj)
        {
            mod.ModId = obj.GetString("id") ?? "";
            mod.Name = obj.GetString("name") ?? mod.ModId;
            mod.Description = obj.GetString("description");
            mod.Version = obj.GetString("version");
            mod.Url = obj.GetObj("contact")?.GetString("homepage");

            var side = obj.GetString("environment")?.ToLower();
            mod.Side = side switch
            {
                null => SideType.None,
                "*" => SideType.Both,
                "client" => SideType.Client,
                "server" => SideType.Server,
                _ => mod.Side
            };

            if (obj.GetArray("authors") is { } list1)
            {
                foreach (var item in list1.ToStringList())
                {
                    mod.Dependants.Add(item);
                }
            }

            if (obj.GetObj("depends") is { } array2)
            {
                foreach (var item3 in array2)
                {
                    mod.Dependants.Add(item3.Key);
                }
            }

            await CheckJarInJarAsync(mod, zFile);

            istest = true;
        }

        async Task QuiltAsync(JsonObject obj)
        {
            mod.ModId = obj.GetString("id") ?? "";
            mod.Version = obj.GetString("version");
            mod.Url = obj.GetObj("contact")?.GetString("homepage");
            var meta = obj.GetObj("metadata");
            if (meta != null)
            {
                mod.Name = meta.GetString("name") ?? mod.ModId;
                mod.Description = meta.GetString("description");

                if (meta.GetObj("contributors") is { } array)
                {
                    var list = array.ToStringList();
                    foreach (var item in list)
                    {
                        mod.Author.Add(item);
                    }
                }
            }
            if (obj.GetArray("depends") is { } obj5)
            {
                foreach (var item3 in obj5)
                {
                    if (item3?.AsObject().GetString("id") is { } str)
                    {
                        mod.Dependants.Add(str);
                    }
                }
            }

            await CheckJarInJarAsync(mod, zFile);
            istest = true;
        }

        //forge 1.13以下
        var item1 = zFile.GetEntry(Names.NameMcModInfoFile);
        if (item1 != null)
        {
            try
            {
                using var stream1 = item1.OpenEntryStream();
                var data = await StringHelper.GetStringAsync(stream1);
                var obj1 = Parse(data);
                JsonObject? obj3;
                if (obj1 is JsonArray array)
                {
                    obj3 = array[0]?.AsObject();
                }
                else
                {
                    obj3 = obj1?.AsObject().GetArray("modList")?[0]?.AsObject();
                }
                if (obj3 != null)
                {
                    ForgeOld(obj3);
                }
            }
            catch (Exception ex)
            {
                Logs.Crash(string.Format(LanguageHelper.Get("Core.Game.Error21"), path), ex);
            }
        }

        //forge 1.13及以上
        bool neoforge = false;
        item1 = zFile.GetEntry(Names.NameMcModTomlFile);
        if (item1 == null)
        {
            //neoforge 1.20.5及以上
            item1 = zFile.GetEntry(Names.NameNeoTomlFile);
            neoforge = true;
            item1 ??= zFile.GetEntry(Names.NameNeoToml1File);
        }
        if (item1 != null)
        {
            mod.Loaders.Add(neoforge ? Loaders.NeoForge : Loaders.Forge);
            if (!istest)
            {
                try
                {
                    using var stream1 = item1.OpenEntryStream();
                    var data = await StringHelper.GetStringAsync(stream1);
                    var model = Toml.Parse(data).ToModel();
                    if (model != null)
                    {
                        await ForgeNewAsync(model);
                    }
                }
                catch (Exception ex)
                {
                    Logs.Crash(string.Format(LanguageHelper.Get("Core.Game.Error21"), path), ex);
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
                    using var stream = item1.OpenEntryStream();
                    var obj1 = await JsonUtils.ReadAsObjAsync(stream);
                    if (obj1 != null)
                    {
                        await FabricAsync(obj1);
                    }
                }
                catch (Exception ex)
                {
                    Logs.Crash(string.Format(LanguageHelper.Get("Core.Game.Error21"), path), ex);
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
                    using var stream = item1.OpenEntryStream();
                    var obj = await JsonUtils.ReadAsObjAsync(stream);
                    if (obj?.GetObj("quilt_loader") is { } obj1)
                    {
                        await QuiltAsync(obj1);
                    }
                }
                catch (Exception ex)
                {
                    Logs.Crash(string.Format(LanguageHelper.Get("Core.Game.Error21"), path), ex);
                }
            }
        }

        try
        {
            //core mod
            item1 = zFile.GetEntry("META-INF/services/cpw.mods.modlauncher.api.ITransformationService");
            var item5 = zFile.GetEntry("META-INF/services/net.minecraftforge.forgespi.language.IModLanguageProvider");
            var item7 = zFile.GetEntry("META-INF/services/net.neoforged.neoforgespi.language.IModLanguageLoader");
            var item6 = zFile.GetEntry("META-INF/MANIFEST.MF");
            if (item6 != null)
            {
                using var stream = item6.OpenEntryStream();
                var con = GameOptions.ReadOptions(stream);
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
                using var stream = item1.OpenEntryStream();
                var obj1 = await JsonUtils.ReadAsObjAsync(stream);
                if (obj1 != null)
                {
                    var obj2 = FindKey(obj1, "acceptedMinecraftVersions");
                    if (obj2?.AsObject() is { } obj3)
                    {
                        mod.CoreMod = true;
                        mod.Loaders.Add(Loaders.Forge);
                        if (!istest)
                        {
                            if (obj3.GetObj("modId")?.GetString("value") is { } str)
                            {
                                mod.ModId = str;
                            }
                            else if (obj3.GetObj("modid")?.GetString("value") is { } str1)
                            {
                                mod.ModId = str1;
                            }

                            if (obj3.GetObj("names")?.GetString("value") is { } str2)
                            {
                                mod.Name = str2;
                            }
                            else if (obj3.GetObj("name")?.GetString("value") is { } str3)
                            {
                                mod.Name = str3;
                            }

                            if (obj3.GetObj("version")?.GetString("value") is { } str4)
                            {
                                mod.Version = str4;
                            }

                            if (obj3.GetObj("dependencies")?.GetString("value") is { } str5)
                            {
                                mod.Dependants.Add(str5);
                            }

                            istest = true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logs.Crash(string.Format(LanguageHelper.Get("Core.Game.Error21"), path), ex);
        }

        if (!istest)
        {
            //使用jarjar的内容
            foreach (var item3 in zFile.Entries)
            {
                if (item3.Key == null || !item3.Key.EndsWith(Names.NameJarExt) || !item3.Key.StartsWith(Names.NameModJarJarDir))
                {
                    continue;
                }

                using var stream = item3.OpenEntryStream();
                using var zFile1 = ZipArchive.Open(stream);
                var inmod = await ReadModAsync(path, zFile1);
                if (inmod != null && !string.IsNullOrWhiteSpace(inmod.Name)
                                  && !string.IsNullOrWhiteSpace(inmod.ModId) && !inmod.CoreMod)
                {
                    return inmod;
                }
            }
        }
        else if (istest)
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
    private static JsonNode? Parse(string data)
    {
        //var options = new JsonSerializerOptions
        //{
        //    ReadCommentHandling = JsonCommentHandling.Skip,
        //    AllowTrailingCommas = true
        //};

        try
        {
            return JsonNode.Parse(data);
        }
        catch (JsonException)
        {
            var s_separator = new[] { "\r\n", "\n" };
            var lines = data.Split(s_separator, StringSplitOptions.RemoveEmptyEntries);

            var rootObject = new JsonObject();
            foreach (var line in lines)
            {
                try
                {
                    var wrappedLine = "{" + line + "}";
                    var parsedLine = JsonNode.Parse(wrappedLine);

                    if (parsedLine is JsonObject lineObj && lineObj.First() is KeyValuePair<string, JsonNode?> firstProp)
                    {
                        var copiedValue = JsonNode.Parse(firstProp.Value!.ToJsonString());
                        rootObject[firstProp.Key] = copiedValue;
                    }
                }
                catch (JsonException)
                {

                }
            }

            return new JsonArray { rootObject };
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
    private static List<string> ToStringList(this JsonArray array)
    {
        var list = new List<string>();
        foreach (var item in array)
        {
            if (item is JsonObject obj && obj.GetString("name") is { } str)
            {
                list.Add(str);
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
    private static List<string> ToStringList(this JsonObject array)
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
    private static JsonNode? FindKey(this JsonObject obj, string key)
    {
        foreach (var item in obj)
        {
            if (item.Key == key)
            {
                return obj;
            }

            if (item.Value is JsonObject obj1
                && FindKey(obj1, key) is { } obj2)
            {
                return obj2;
            }
            else if (item.Value is JsonArray arry
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
    private static JsonNode? FindKey(this JsonArray obj, string key)
    {
        foreach (var item in obj)
        {
            if (item is JsonObject obj1)
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
