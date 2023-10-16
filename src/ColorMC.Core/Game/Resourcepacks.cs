using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ColorMC.Core.Game;

/// <summary>
/// 材质包相关操作
/// </summary>
public static class Resourcepacks
{
    private static async Task<ResourcepackObj?> ReadResourcepack(Stream file, CancellationToken cancel)
    {
        using var zFile = new ZipFile(file);
        var item1 = zFile.GetEntry("pack.mcmeta");
        if (item1 != null)
        {
            using var stream1 = zFile.GetInputStream(item1);
            using var stream = new MemoryStream();
            await stream1.CopyToAsync(stream, cancel);
            var data = Encoding.UTF8.GetString(stream.ToArray());
            var obj1 = JObject.Parse(data);
            if (obj1 != null)
            {
                var obj = new ResourcepackObj();
                if (obj1.ContainsKey("pack"))
                {
                    var obj2 = obj1["pack"] as JObject;
                    if (obj2!["pack_format"] is { } item2)
                    {
                        obj.pack_format = (int)item2;
                    }
                    if (obj2.ContainsKey("description"))
                    {
                        var obj3 = obj2["description"]!;
                        if (obj3.Type == JTokenType.String)
                        {
                            obj.description = obj3.ToString();
                        }
                        else if (obj3.Type == JTokenType.Object)
                        {
                            obj.description = obj3["fallback"]?.ToString() ?? "";
                        }
                    }
                }
                item1 = zFile.GetEntry("pack.png");
                if (item1 != null)
                {
                    using var stream2 = zFile.GetInputStream(item1);
                    using var stream3 = new MemoryStream();
                    await stream2.CopyToAsync(stream3, cancel);
                    obj.Icon = stream3.ToArray();
                }
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取材质包列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>列表</returns>
    public static async Task<List<ResourcepackObj>> GetResourcepacks(this GameSettingObj game, bool sha256)
    {
        var list = new List<ResourcepackObj>();
        var dir = game.GetResourcepacksPath();

        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return list;
        }

        await Parallel.ForEachAsync(info.GetFiles(), async (item, cancel) =>
        {
            using var stream = PathHelper.OpenRead(item.FullName)!;
            string sha1 = HashHelper.GenSha1(stream);
            if (item.Extension is not (".zip" or ".disable"))
            {
                return;
            }
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var obj = await ReadResourcepack(stream, cancel);
                if (obj != null)
                {
                    obj.Local = Path.GetFullPath(item.FullName);
                    obj.Sha1 = sha1;
                    if (sha256)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        obj.Sha256 = HashHelper.GenSha256(stream);
                    }
                    list.Add(obj);
                }
                else
                {
                    list.Add(new()
                    {
                        Sha1 = sha1,
                        Local = Path.GetFullPath(item.FullName),
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
    public static async Task<bool> AddResourcepack(this GameSettingObj obj, List<string> file)
    {
        var path = obj.GetResourcepacksPath();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var ok = true;
        await Parallel.ForEachAsync(file, async (item, cancel) =>
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
            }, cancel);
        });
        return ok;
    }
}
