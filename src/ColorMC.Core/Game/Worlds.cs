using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Core.Game;

/// <summary>
/// 世界相关操作
/// </summary>
public static class Worlds
{
    private const string Name1 = "datapacks";
    private const string Name2 = "colormc.info.json";

    /// <summary>
    /// 获取世界列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>世界列表</returns>
    public static async Task<List<WorldObj>> GetWorldsAsync(this GameSettingObj game)
    {
        var list = new List<WorldObj>();
        var dir = game.GetSavesPath();

        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return list;
        }

        await Parallel.ForEachAsync(info.GetDirectories(), async (item, cacenl) =>
        {
            var world = await ReadWorld(item);
            if (world != null)
            {
                list.Add(world);
            }
        });

        return list;
    }

    /// <summary>
    /// 删除世界
    /// </summary>
    /// <param name="world">世界储存</param>
    public static void Remove(this WorldObj world)
    {
        string dir = Path.GetFullPath(world.Game.GetRemoveWorldPath());
        Directory.CreateDirectory(dir);
        Directory.Move(world.Local, Path.GetFullPath(dir + "/" + world.LevelName));
    }

    /// <summary>
    /// 导入世界压缩包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件位置</param>
    /// <returns>结果</returns>
    public static async Task<bool> AddWorldZipAsync(this GameSettingObj obj, string file)
    {
        var dir = obj.GetSavesPath();
        var info = new FileInfo(file);
        dir = Path.GetFullPath(dir + "/" + info.Name[..^info.Extension.Length] + "/");
        Directory.CreateDirectory(dir);
        try
        {
            using ZipFile zFile = new(file);
            using var stream1 = new MemoryStream();
            var dir1 = "";
            var find = false;
            foreach (ZipEntry e in zFile)
            {
                if (e.IsFile && e.Name.EndsWith("level.dat"))
                {
                    dir1 = e.Name.Replace("level.dat", "");
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                return false;
            }

            foreach (ZipEntry e in zFile)
            {
                if (e.IsFile)
                {
                    using var stream = zFile.GetInputStream(e);
                    var file1 = Path.GetFullPath(dir + e.Name[dir1.Length..]);
                    var info2 = new FileInfo(file1);
                    info2.Directory?.Create();
                    using FileStream stream3 = new(file1, FileMode.Create,
                        FileAccess.ReadWrite, FileShare.ReadWrite);
                    await stream.CopyToAsync(stream3);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Pack.Error2"), e);
        }

        return false;
    }

    /// <summary>
    /// 导出世界
    /// </summary>
    /// <param name="world">世界实例</param>
    /// <param name="file">输出文件位置</param>
    public static Task ExportWorldZip(this WorldObj world, string file)
    {
        return new ZipUtils().ZipFileAsync(world.Local, file);
    }

    /// <summary>
    /// 获取世界数据包储存
    /// </summary>
    /// <param name="world">世界储存</param>
    /// <returns>位置</returns>
    public static string GetWorldDataPacksPath(this WorldObj world)
    {
        return Path.GetFullPath($"{world.Local}/{Name1}");
    }

    /// <summary>
    /// 备份世界
    /// </summary>
    /// <param name="world">世界储存</param>
    public static async Task BackupAsync(this WorldObj world)
    {
        var game = world.Game;

        var path = game.GetWorldBackupPath();
        Directory.CreateDirectory(path);

        var file = Path.GetFullPath(path + "/" + world.LevelName + "_" + DateTime.Now
            .ToString("yyyy_MM_dd_HH_mm_ss") + ".zip");

        await new ZipUtils().ZipFileAsync(world.Local, file);
        using var s = new ZipFile(PathHelper.OpenWrite(file, false));
        var info = new { name = world.LevelName };
        var data = JsonConvert.SerializeObject(info);
        var data1 = Encoding.UTF8.GetBytes(data);
        using var stream = new ZipFileStream(data1);
        s.BeginUpdate();
        s.Add(stream, Name2);
        s.CommitUpdate();
    }

    /// <summary>
    /// 还原世界
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="item1">文件</param>
    /// <returns>还原结果</returns>
    public static async Task<bool> UnzipBackupWorldAsync(this GameSettingObj obj, UnzipBackupWorldArg arg)
    {
        var local = "";
        var res = false;

        using var s = new ZipInputStream(PathHelper.OpenRead(arg.File));
        using var stream1 = new MemoryStream();
        ZipEntry theEntry;
        while ((theEntry = s.GetNextEntry()) != null)
        {
            if (theEntry.Name == Name2)
            {
                await s.CopyToAsync(stream1);
                res = true;
                break;
            }
        }
        if (!res)
        {
            return false;
        }
        var data = stream1.ToArray();
        var data1 = Encoding.UTF8.GetString(data);
        var info = JObject.Parse(data1);
        var name = info?["name"]?.ToString();
        if (name == null)
        {
            return false;
        }

        var list1 = await obj.GetWorldsAsync();
        var item = list1.FirstOrDefault(a => a.LevelName == name);

        if (item != null)
        {
            local = item.Local;
            await PathHelper.DeleteFilesAsync(new DeleteFilesArg
            {
                Local = item.Local,
                Request = arg.Request
            });
        }
        else
        {
            local = Path.GetFullPath(obj.GetSavesPath() + "/" + name);
        }

        try
        {
            await new ZipUtils().UnzipAsync(local, arg.File,
                PathHelper.OpenRead(arg.File)!);
            return true;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(LanguageHelper.Get("Core.Game.Error11"), e, false);
            return false;
        }
    }

    /// <summary>
    /// 读取世界信息
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    private static async Task<WorldObj?> ReadWorld(DirectoryInfo dir)
    {
        var file = Path.GetFullPath(dir.FullName + "/level.dat");
        if (!File.Exists(file))
        {
            return null;
        }

        try
        {
            //读NBT
            if (await NbtBase.Read(file) is not NbtCompound tag)
            {
                throw new Exception("NBT tag error");
            }

            var obj = new WorldObj()
            {
                Nbt = tag
            };

            //读数据
            var tag1 = tag.TryGet<NbtCompound>("Data")!;
            obj.LastPlayed = tag1.TryGet<NbtLong>("LastPlayed")!.Value;
            obj.GameType = tag1.TryGet<NbtInt>("GameType")!.Value;
            obj.Hardcore = tag1.TryGet<NbtByte>("hardcore")!.Value;
            obj.Difficulty = tag1.TryGet<NbtByte>("Difficulty")!.Value;
            obj.LevelName = tag1.TryGet<NbtString>("LevelName")!.Value;

            obj.Local = Path.GetFullPath(dir.FullName);

            var icon = dir.GetFiles().Where(a => a.Name == "icon.png").FirstOrDefault();
            if (icon != null)
            {
                obj.Icon = icon.FullName;
            }

            return obj;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error4"), e);
        }

        return new()
        {
            Broken = true,
            Local = Path.GetFullPath(dir.FullName)
        };
    }
}
