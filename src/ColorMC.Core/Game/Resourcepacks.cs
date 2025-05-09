using System.IO.Compression;
using System.Text.Json;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

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
            if (item.Extension is not Names.NameZipExt)
            {
                return;
            }
            try
            {
                using var stream = PathHelper.OpenRead(item.FullName);
                if (stream == null)
                {
                    return;
                }
                string sha1 = await HashHelper.GenSha1Async(stream);
                string filesha256 = "";
                if (sha256)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    filesha256 = await HashHelper.GenSha256Async(stream);
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
                        Sha256 = filesha256,
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
        PathHelper.Delete(obj.Local);
    }

    /// <summary>
    /// 获取材质包
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="file">取消Token</param>
    /// <returns>材质包</returns>
    private static async Task<ResourcepackObj?> ReadResourcepackAsync(Stream stream1, CancellationToken cancel)
    {
        using var zFile = new ZipArchive(stream1);
        var item1 = zFile.GetEntry(Names.NamePackMetaFile);
        if (item1 == null)
        {
            return null;
        }

        using var stream = item1.Open();
        var obj1 = await JsonUtils.ReadAsObjAsync(stream);
        if (obj1 == null)
        {
            return null;
        }

        var obj = new ResourcepackObj();
        if (obj1.GetObj("pack") is { } obj2)
        {
            obj.PackFormat = obj2.GetInt("pack_format") ?? -1;
            if (obj2.TryGetPropertyValue("description", out var obj3)
                && obj3 != null)
            {
                if (obj3.GetValueKind() == JsonValueKind.String)
                {
                    obj.Description = obj3.GetValue<string>();
                }
                else if (obj3.GetValueKind() == JsonValueKind.Object)
                {
                    obj.Description = obj3?.AsObject()?.GetString("fallback") ?? "";
                }
            }
        }
        item1 = zFile.GetEntry(Names.NamePackIconFile);
        if (item1 != null)
        {
            using var stream2 = item1.Open();
            using var stream3 = new MemoryStream();
            await stream2.CopyToAsync(stream3, cancel);
            obj.Icon = stream3.ToArray();
        }
        return obj;
    }
}
