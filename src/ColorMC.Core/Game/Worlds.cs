using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 世界相关操作
/// </summary>
public static class Worlds
{
    /// <summary>
    /// 获取世界列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>世界列表</returns>
    public static async Task<List<WorldObj>> GetWorldsAsync(this GameSettingObj game)
    {
        var dir = game.GetSavesPath();
        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            return [];
        }

        var list = new ConcurrentBag<WorldObj>();
        await Parallel.ForEachAsync(info.GetDirectories(), async (item, cacenl) =>
        {
            var world = await ReadWorld(item);
            if (world != null)
            {
                world.Game = game;
                list.Add(world);
            }
        });

        return [.. list];
    }

    /// <summary>
    /// 删除世界
    /// </summary>
    /// <param name="world">世界储存</param>
    public static void Remove(this WorldObj world)
    {
        string dir = world.Game.GetRemoveWorldPath();
        Directory.CreateDirectory(dir);
        Directory.Move(world.Local, Path.Combine(dir, world.LevelName));
    }

    /// <summary>
    /// 导入世界
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="name">名字</param>
    /// <param name="file">导入的文件</param>
    /// <returns>是否成功导入</returns>
    public static async Task<bool> AddWorldZipAsync(this GameSettingObj obj, string name, Stream file)
    {
        var dir = obj.GetSavesPath();
        var info = new FileInfo(name);
        dir = Path.Combine(dir, info.Name[..^info.Extension.Length]);
        Directory.CreateDirectory(dir);
        return await Task.Run(() =>
        {
            try
            {
                using var zFile = new ZipArchive(file);
                var dir1 = "";
                var find = false;
                foreach (var e in zFile.Entries)
                {
                    if (e.Name == Names.NameLevelFile)
                    {
                        dir1 = e.FullName.Replace(Names.NameLevelFile, "");
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    return false;
                }

                foreach (var e in zFile.Entries)
                {
                    var file1 = Path.Combine(dir, e.FullName[dir1.Length..]);
                    e.ExtractToFile(file1, true);
                }

                return true;
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Pack.Error2"), e);
            }
            return false;
        });
    }

    /// <summary>
    /// 导入世界压缩包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件位置</param>
    /// <returns>是否成功导入</returns>
    public static async Task<bool> AddWorldZipAsync(this GameSettingObj obj, string file)
    {
        using var stream = PathHelper.OpenRead(file);
        if (stream == null)
        {
            return false;
        }
        return await obj.AddWorldZipAsync(file, stream);
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
        return Path.Combine(world.Local, Names.NameGameDatapackDir);
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

        var file = Path.Combine(path, world.LevelName + "_" + DateTime.Now
            .ToString("yyyy_MM_dd_HH_mm_ss") + ".zip");

        await new ZipUtils().ZipFileAsync(world.Local, file);
    }

    /// <summary>
    /// 还原世界
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="arg">参数</param>
    /// <returns>是否成功还原</returns>
    public static async Task<bool> UnzipBackupWorldAsync(this GameSettingObj obj, UnzipBackupWorldArg arg)
    {
        var local = Path.Combine(obj.GetSavesPath(), Path.GetFileName(arg.File));
        if (Directory.Exists(local))
        {
            await PathHelper.DeleteFilesAsync(new DeleteFilesArg
            {
                Local = local,
                Request = arg.Request
            });
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
    /// <param name="dir">文件夹</param>
    /// <returns>世界储存</returns>
    private static async Task<WorldObj?> ReadWorld(DirectoryInfo dir)
    {
        var file = Path.Combine(dir.FullName, Names.NameLevelFile);
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

            obj.Local = dir.FullName;

            var icon = dir.GetFiles().Where(a => a.Name == Names.NameIconFile).FirstOrDefault();
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
            Local = dir.FullName
        };
    }
}
