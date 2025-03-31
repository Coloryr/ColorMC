using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Game;

/// <summary>
/// 材质包相关操作
/// </summary>
public static class Resourcepacks
{
    /// <summary>
    /// 获取材质包列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="sha256">是否获取SHA256</param>
    /// <returns>材质包列表</returns>
    public static async Task<List<ResourcepackObj>> GetResourcepacksAsync(this GameSettingObj game, bool sha256)
    {
        var dir = game.GetResourcepacksPath();
        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return [];
        }
        var list = new List<ResourcepackObj>();

        var dirs = info.GetDirectories();

        await Parallel.ForEachAsync(dirs, (item, cancel) =>
        {
            try
            {
                var obj = ReadResourcepack(item);
                if (obj != null)
                {
                    obj.IsDir = true;
                    obj.Local = item.FullName;
                    list.Add(obj);
                }
            }
            catch
            {

            }

            return ValueTask.CompletedTask;
        });

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
            using var stream = PathHelper.OpenRead(item.FullName)!;
            string sha1 = HashHelper.GenSha1(stream);
            if (item.Extension is not Names.NameZipExt)
            {
                return;
            }
            try
            {
                string filesha256 = "";
                if (sha256)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    filesha256 = HashHelper.GenSha256(stream);
                }
                stream.Seek(0, SeekOrigin.Begin);
                var obj = await ReadResourcepackAsync(stream, cancel);
                if (obj != null)
                {
                    obj.Local = item.FullName;
                    obj.Sha1 = sha1;
                    obj.Sha256 = filesha256;
                    list.Add(obj);
                }
                else
                {
                    list.Add(new()
                    {
                        Sha1 = sha1,
                        Local = item.FullName,
                        Broken = true
                    });
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Game.Error2"), e);
            }
        });

        return list;
    }

    /// <summary>
    /// 导入材质包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">导入列表</param>
    /// <returns>结果</returns>
    public static async Task<bool> AddResourcepackAsync(this GameSettingObj obj, List<string> file)
    {
        var path = obj.GetResourcepacksPath();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var ok = true;
        await Parallel.ForEachAsync(file, async (item, cancel) =>
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
            }, cancel);
        });
        return ok;
    }

    /// <summary>
    /// 删除材质包
    /// </summary>
    /// <param name="obj">材质包</param>
    public static void Delete(this ResourcepackObj obj)
    {
        if (obj.IsDir)
        {
            _ = PathHelper.DeleteFilesAsync(new DeleteFilesArg()
            { 
                Local = obj.Local 
            });
        }
        else
        {
            PathHelper.Delete(obj.Local);
        }
    }

    /// <summary>
    /// 获取材质包
    /// </summary>
    /// <param name="file">文件流</param>
    /// <param name="file">取消Token</param>
    /// <returns>材质包</returns>
    private static async Task<ResourcepackObj?> ReadResourcepackAsync(Stream file, CancellationToken cancel)
    {
        using var zFile = new ZipFile(file);
        var item1 = zFile.GetEntry(Names.NamePackMetaFile);
        if (item1 == null)
        {
            return null;
        }

        using var stream1 = zFile.GetInputStream(item1);
        var data = await StringHelper.GetStringAsync(stream1);
        var obj1 = JObject.Parse(data);
        if (obj1 == null)
        {
            return null;
        }

        var obj = ReadObj(obj1);
        item1 = zFile.GetEntry(Names.NamePackIconFile);
        if (item1 != null)
        {
            using var stream2 = zFile.GetInputStream(item1);
            using var stream3 = new MemoryStream();
            await stream2.CopyToAsync(stream3, cancel);
            obj.Icon = stream3.ToArray();
        }
        return obj;
    }

    /// <summary>
    /// 获取材质包
    /// </summary>
    /// <param name="file">文件流</param>
    /// <param name="file">取消Token</param>
    /// <returns>材质包</returns>
    private static ResourcepackObj? ReadResourcepack(DirectoryInfo dir)
    {
        var file = Path.Combine(dir.FullName, Names.NamePackMetaFile);
        if (!File.Exists(file))
        {
            return null;
        }
        var data = PathHelper.ReadText(file);
        if (data == null)
        {
            return null;
        }
        var obj1 = JObject.Parse(data);
        if (obj1 == null)
        {
            return null;
        }
        var obj = ReadObj(obj1);

        file = Path.Combine(dir.FullName, Names.NamePackIconFile);
        if (File.Exists(file))
        {
            obj.Icon = PathHelper.ReadByte(file);
        }
        return obj;
    }

    /// <summary>
    /// 读取材质包信息
    /// </summary>
    /// <param name="obj1">json信息</param>
    /// <returns></returns>
    private static ResourcepackObj ReadObj(JObject obj1)
    {
        var obj = new ResourcepackObj();
        if (obj1.ContainsKey("pack"))
        {
            var obj2 = obj1["pack"] as JObject;
            if (obj2!["pack_format"] is { } item2)
            {
                obj.PackFormat = (int)item2;
            }
            if (obj2.ContainsKey("description"))
            {
                var obj3 = obj2["description"]!;
                if (obj3.Type == JTokenType.String)
                {
                    obj.Description = obj3.ToString();
                }
                else if (obj3.Type == JTokenType.Object)
                {
                    obj.Description = obj3["fallback"]?.ToString() ?? "";
                }
            }
        }

        return obj;
    }
}
