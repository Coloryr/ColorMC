using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using fNbt;
using ICSharpCode.SharpZipLib.Zip;

namespace ColorMC.Core.Game;

public enum WorldSave
{
    LevelName, Raining, MapFeatures, RandomSeed,
    SpawnX, SpawnY, SpawnZ, LastPlayed, GameType,
    ThunderTime, Version, RainTime, Time,
    Thundering, Hardcore, SizeOnDisk
}

public static class Worlds
{
    public static async Task<List<WorldObj>> GetWorlds(this GameSettingObj game)
    {
        List<WorldObj> list = new();
        string dir = game.GetSavesPath();

        DirectoryInfo info = new(dir);
        if (!info.Exists)
            return list;

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = 5
        };
        await Parallel.ForEachAsync(info.GetDirectories(), options, (item, cacenl) =>
        {
            bool find = false;
            foreach (var item1 in item.GetFiles())
            {
                if (item1.Name != "level.dat")
                    continue;

                try
                {
                    WorldObj obj = new();

                    var myFile = new NbtFile();
                    myFile.LoadFromFile(item1.FullName);
                    var myCompoundTag = myFile.RootTag as NbtCompound;
                    var tag1 = myCompoundTag.Get<NbtCompound>("Data");
                    obj.LastPlayed = tag1.Get<NbtLong>("LastPlayed").Value;
                    obj.GameType = tag1.Get<NbtInt>("GameType").Value;
                    obj.Hardcore = tag1.Get<NbtByte>("hardcore").Value;
                    obj.Difficulty = tag1.Get<NbtByte>("Difficulty").Value;
                    obj.LevelName = tag1.Get<NbtString>("LevelName").Value;

                    //CompoundTag tag = await NbtFile.ReadAsync(item1.FullName, 
                    //    FormatOptions.None, CompressionType.AutoDetect);
                    //var tag1 = tag["Data"] as CompoundTag;
                    //if (tag1 == null)
                    //    break;

                    ////obj.Raining = (byte)((tag1["raining"] as ByteTag)?.Value);
                    ////    obj.RandomSeed = (long)((tag1["RandomSeed"] as LongTag)?.Value);
                    ////    obj.SpawnX = (int)((tag1["SpawnX"] as IntTag)?.Value);
                    ////obj.SpawnZ = (int)((tag1["SpawnZ"] as IntTag)?.Value);
                    //obj.LastPlayed = (long)((tag1["LastPlayed"] as LongTag)?.Value);
                    //obj.GameType = (int)((tag1["GameType"] as IntTag)?.Value);
                    ////    obj.SpawnY = (int)((tag1["SpawnY"] as IntTag)?.Value);
                    ////    obj.MapFeatures = (byte)((tag1["MapFeatures"] as ByteTag)?.Value);
                    ////    obj.ThunderTime = (int)((tag1["thunderTime"] as IntTag)?.Value);
                    ////    obj.Version = (int)((tag1["version"] as IntTag)?.Value);
                    ////    obj.RainTime = (int)((tag1["rainTime"] as IntTag)?.Value);
                    ////    obj.Time = (long)((tag1["Time"] as LongTag)?.Value);
                    ////    obj.Thundering = (byte)((tag1["thundering"] as ByteTag)?.Value);
                    //obj.Hardcore = (byte)((tag1["hardcore"] as ByteTag)?.Value);
                    //obj.Difficulty = (byte)((tag1["Difficulty"] as ByteTag)?.Value);
                    ////    obj.SizeOnDisk = (long)((tag1["SizeOnDisk"] as LongTag)?.Value);
                    //obj.LevelName = (tag1["LevelName"] as StringTag)?.Value;

                    obj.Local = Path.GetFullPath(item.FullName);
                    obj.Game = game;

                    var icon = item.GetFiles().Where(a => a.Name == "icon.png").FirstOrDefault();
                    if (icon != null)
                    {
                        obj.Icon = File.ReadAllBytes(icon.FullName);
                    }

                    list.Add(obj);
                    find = true;
                    break;
                }
                catch (Exception e)
                {
                    Logs.Error("地图读取失败", e);
                }

                if (!find)
                {
                    list.Add(new()
                    {
                        Broken = true,
                        Local = Path.GetFullPath(item.FullName),
                        Game = game
                    });
                }
            }

            return ValueTask.CompletedTask;
        });

        return list;
    }

    public static void Remove(this WorldObj world)
    {
        string dir = Path.GetFullPath(world.Game.GetBasePath() + "/remove_worlds");
        Directory.CreateDirectory(dir);
        Directory.Move(world.Local, Path.GetFullPath(dir + world.LevelName));
    }

    public static async Task<bool> ImportWorldZip(this GameSettingObj obj, string file)
    {
        var dir = obj.GetSavesPath();
        var info = new FileInfo(file);
        dir = Path.GetFullPath(dir + "/" + info.Name[..^info.Extension.Length] + "/");
        Directory.CreateDirectory(dir);
        try
        {
            using ZipFile zFile = new(file);
            using var stream1 = new MemoryStream();
            string dir1 = "";
            bool find = false;
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
                    string file1 = Path.GetFullPath(dir + e.Name[dir1.Length..]);
                    FileInfo info2 = new(file1);
                    info2.Directory.Create();
                    using FileStream stream3 = new(file1, FileMode.Create,
                        FileAccess.ReadWrite, FileShare.ReadWrite);
                    await stream.CopyToAsync(stream3);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Path.Instances.Load.Error"), e);
        }

        return false;
    }

    public static Task ExportWorldZip(this WorldObj world, string file)
    {
        return ZipFloClass.ZipFile(world.Local, file);
    }

    //public static async Task Save(this WorldObj world, List<WorldSave> list)
    //{
    //    CompoundTag tag = await NbtFile.ReadAsync(world.Local + "/level.dat", FormatOptions.Java, CompressionType.AutoDetect);
    //    var tag1 = tag["Data"] as CompoundTag;
    //    if (tag1 == null)
    //        return;

    //    foreach (var item in list)
    //    {
    //        switch (item)
    //        {
    //            case WorldSave.Raining:
    //                tag1.Add(new ByteTag("raining", world.Raining));
    //                break;
    //            case WorldSave.RandomSeed:
    //                tag1.Add(new LongTag("RandomSeed", world.RandomSeed));
    //                break;
    //            case WorldSave.SpawnX:
    //                tag1.Add(new IntTag("SpawnX", world.SpawnX));
    //                break;
    //            case WorldSave.SpawnY:
    //                tag1.Add(new IntTag("SpawnY", world.SpawnY));
    //                break;
    //            case WorldSave.SpawnZ:
    //                tag1.Add(new IntTag("SpawnZ", world.SpawnZ));
    //                break;
    //            case WorldSave.LastPlayed:
    //                tag1.Add(new LongTag("LastPlayed", world.LastPlayed));
    //                break;
    //            case WorldSave.GameType:
    //                tag1.Add(new IntTag("GameType", world.GameType));
    //                break;
    //            case WorldSave.MapFeatures:
    //                tag1.Add(new ByteTag("MapFeatures", world.MapFeatures));
    //                break;
    //            case WorldSave.ThunderTime:
    //                tag1.Add(new IntTag("thunderTime", world.ThunderTime));
    //                break;
    //            case WorldSave.Version:
    //                tag1.Add(new IntTag("version", world.Version));
    //                break;
    //            case WorldSave.RainTime:
    //                tag1.Add(new IntTag("rainTime", world.RainTime));
    //                break;
    //            case WorldSave.Time:
    //                tag1.Add(new LongTag("Time", world.Time));
    //                break;
    //            case WorldSave.Thundering:
    //                tag1.Add(new ByteTag("thundering", world.Thundering));
    //                break;
    //            case WorldSave.Hardcore:
    //                tag1.Add(new ByteTag("hardcore", world.Hardcore));
    //                break;
    //            case WorldSave.SizeOnDisk:
    //                tag1.Add(new LongTag("SizeOnDisk", world.SizeOnDisk));
    //                break;
    //            case WorldSave.LevelName:
    //                tag1.Add(new StringTag("LevelName", world.LevelName));
    //                break;
    //        }
    //    }

    //    tag.Add(tag1);
    //    await NbtFile.WriteAsync(world.Local + "/level.dat", tag, FormatOptions.Java, CompressionType.AutoDetect);
    //}
}
