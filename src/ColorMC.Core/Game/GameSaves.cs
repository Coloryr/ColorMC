using System.Collections.Concurrent;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using SharpCompress.Archives.Zip;

namespace ColorMC.Core.Game;

/// <summary>
/// 存档相关操作
/// </summary>
public static class GameSaves
{
    /// <summary>
    /// 获取存档列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>存档列表</returns>
    public static async Task<List<SaveObj>> GetSavesAsync(this GameSettingObj game)
    {
        var dir = game.GetSavesPath();
        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            return [];
        }

        var list = new ConcurrentBag<SaveObj>();
        await Parallel.ForEachAsync(info.GetDirectories(), async (item, cacenl) =>
        {
            var world = await ReadSaveAsync(game, item);
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
    public static Task DeleteAsync(this SaveObj world)
    {
        return PathHelper.MoveToTrashAsync(world.Local);
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
        return await Task.Run(async () =>
        {
            try
            {
                using var zFile = ZipArchive.Open(file);
                var dir1 = "";
                var find = false;
                foreach (var e in zFile.Entries)
                {
                    if (!FunctionUtils.IsFile(e))
                    {
                        continue;
                    }
                    if (e.Key!.EndsWith(Names.NameLevelFile))
                    {
                        dir1 = e.Key.Replace(Names.NameLevelFile, "");
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
                    if (!FunctionUtils.IsFile(e))
                    {
                        continue;
                    }
                    var file1 = Path.Combine(dir, e.Key![dir1.Length..]);
                    using var stream = e.OpenEntryStream();
                    await PathHelper.WriteBytesAsync(file1, stream);
                }

                return true;
            }
            catch (Exception e)
            {
                ColorMCCore.OnError(new GameSaveAddErrorEventArgs(obj, name, e));
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
    /// <param name="save">存档</param>
    /// <param name="file">输出文件位置</param>
    public static async Task ExportSaveZip(this SaveObj save, string file)
    {
        using var stream = PathHelper.OpenWrite(file);
        using var zip = await new ZipProcess().ZipFileAsync(save.Local, stream);
    }

    /// <summary>
    /// 获取存档数据包文件夹
    /// </summary>
    /// <param name="save">存档储存</param>
    /// <returns>文件夹位置</returns>
    public static string GetSaveDataPacksPath(this SaveObj save)
    {
        return Path.Combine(save.Local, Names.NameGameDatapackDir);
    }

    /// <summary>
    /// 备份世界
    /// </summary>
    /// <param name="world">世界储存</param>
    public static async Task BackupAsync(this SaveObj world)
    {
        var game = world.Game;

        var path = game.GetSaveBackupPath();
        Directory.CreateDirectory(path);

        var file = Path.Combine(path, world.LevelName + "_" + DateTime.Now
            .ToString("yyyy_MM_dd_HH_mm_ss") + ".zip");

        using var stream = PathHelper.OpenWrite(file);
        using var zip = await new ZipProcess().ZipFileAsync(world.Local, stream);
    }

    /// <summary>
    /// 还原世界
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="arg">参数</param>
    /// <returns>是否成功还原</returns>
    public static async Task<bool> UnzipBackupWorldAsync(this GameSettingObj obj, string file, IZipGui? gui)
    {
        var local = Path.Combine(obj.GetSavesPath(), Path.GetFileName(file));
        if (Directory.Exists(local))
        {
            await PathHelper.MoveToTrashAsync(local);
        }
        try
        {
            await new ZipProcess(gui).UnzipAsync(local, file,
                PathHelper.OpenRead(file)!);
            return true;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new GameSaveRestoreErrorEventArgs(obj, file, e));
            return false;
        }
    }

    /// <summary>
    /// 读取存档信息
    /// </summary>
    /// <param name="dir">存档文件夹</param>
    /// <returns>存档</returns>
    private static async Task<SaveObj?> ReadSaveAsync(GameSettingObj game, DirectoryInfo dir)
    {
        var file = Path.Combine(dir.FullName, Names.NameLevelFile);
        if (!File.Exists(file))
        {
            return null;
        }

        var obj = new SaveObj()
        {
            Local = dir.FullName
        };

        try
        {
            //读NBT
            if (await NbtBase.ReadAsync(file) is not NbtCompound tag)
            {
                throw new Exception("NBT tag error");
            }

            obj.Nbt = tag;

            //读数据
            var tag1 = tag.TryGet<NbtCompound>("Data")!;
            obj.LastPlayed = tag1.TryGet<NbtLong>("LastPlayed")!.ValueLong;
            if (tag1.TryGet<NbtLong>("RandomSeed") is { } seed)
            {
                obj.RandomSeed = seed.ValueLong;
            }
            if (tag1.TryGet<NbtCompound>("WorldGenSettings") is { } setting)
            {
                if (setting.TryGet<NbtLong>("seed") is { } seed1)
                {
                    obj.RandomSeed = seed1.ValueLong;
                }
                if (setting.TryGet<NbtCompound>("dimensions")?
                    .TryGet<NbtCompound>("minecraft:overworld")?
                    .TryGet<NbtCompound>("generator")?
                    .TryGet<NbtString>("settings") is { } name)
                {
                    obj.GeneratorName = name.Value;
                }
            }
            if (tag1.TryGet<NbtString>("generatorName") is { } name1)
            {
                obj.GeneratorName = "minecraft:" + name1.Value;
            }

            obj.GameType = tag1.TryGet<NbtInt>("GameType")!.ValueInt;
            obj.Hardcore = tag1.TryGet<NbtByte>("hardcore")!.ValueByte;
            obj.Difficulty = tag1.TryGet<NbtByte>("Difficulty")!.ValueByte;
            obj.LevelName = tag1.TryGet<NbtString>("LevelName")!.Value;

            var icon = dir.GetFiles().Where(a => a.Name == Names.NameIconFile).FirstOrDefault();
            if (icon != null)
            {
                obj.Icon = icon.FullName;
            }

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new GameSaveReadErrorEventArgs(game, dir.FullName, e));
        }

        //无法读取
        obj.Broken = true;

        return obj;
    }
}
