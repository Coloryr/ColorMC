using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using SharpNBT;

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
            MaxDegreeOfParallelism = 1
        };
        await Parallel.ForEachAsync(info.GetDirectories(), options, async (item, cacenl) =>
        {
            foreach (var item1 in item.GetFiles())
            {
                if (item1.Name != "level.dat")
                    continue;

                CompoundTag tag = await NbtFile.ReadAsync(item1.FullName, FormatOptions.Java, CompressionType.AutoDetect);
                var tag1 = tag["Data"] as CompoundTag;
                if (tag1 == null)
                    return;

                WorldObj obj = new();
                //obj.Raining = (byte)((tag1["raining"] as ByteTag)?.Value);
                //    obj.RandomSeed = (long)((tag1["RandomSeed"] as LongTag)?.Value);
                //    obj.SpawnX = (int)((tag1["SpawnX"] as IntTag)?.Value);
                //obj.SpawnZ = (int)((tag1["SpawnZ"] as IntTag)?.Value);
                //    obj.LastPlayed = (long)((tag1["LastPlayed"] as LongTag)?.Value);
                //    obj.GameType = (int)((tag1["GameType"] as IntTag)?.Value);
                //    obj.SpawnY = (int)((tag1["SpawnY"] as IntTag)?.Value);
                //    obj.MapFeatures = (byte)((tag1["MapFeatures"] as ByteTag)?.Value);
                //    obj.ThunderTime = (int)((tag1["thunderTime"] as IntTag)?.Value);
                //    obj.Version = (int)((tag1["version"] as IntTag)?.Value);
                //    obj.RainTime = (int)((tag1["rainTime"] as IntTag)?.Value);
                //    obj.Time = (long)((tag1["Time"] as LongTag)?.Value);
                //    obj.Thundering = (byte)((tag1["thundering"] as ByteTag)?.Value);
                //    obj.Hardcore = (byte)((tag1["hardcore"] as ByteTag)?.Value);
                //    obj.SizeOnDisk = (long)((tag1["SizeOnDisk"] as LongTag)?.Value);
                //    obj.LevelName = (tag1["LevelName"] as StringTag)?.Value;
                obj.Local = Path.GetFullPath(item.FullName);
                obj.Game = game;

                var icon = item.GetFiles().Where(a => a.Name == "icon.png").FirstOrDefault();
                if (icon != null)
                {
                    obj.Icon = File.ReadAllBytes(icon.FullName);
                }

                list.Add(obj);
            }
        });

        return list;
    }

    public static void Remove(this WorldObj world)
    {
        string dir = Path.GetFullPath(world.Game.GetBaseDir() + "/remove_worlds");
        Directory.CreateDirectory(dir);
        Directory.Move(world.Local, Path.GetFullPath(dir + "/" + world.LevelName));
    }

    public static async Task Save(this WorldObj world, List<WorldSave> list)
    {
        CompoundTag tag = await NbtFile.ReadAsync(world.Local + "/level.dat", FormatOptions.Java, CompressionType.AutoDetect);
        var tag1 = tag["Data"] as CompoundTag;
        if (tag1 == null)
            return;

        foreach (var item in list)
        {
            switch (item)
            {
                case WorldSave.Raining:
                    tag1.Add(new ByteTag("raining", world.Raining));
                    break;
                case WorldSave.RandomSeed:
                    tag1.Add(new LongTag("RandomSeed", world.RandomSeed));
                    break;
                case WorldSave.SpawnX:
                    tag1.Add(new IntTag("SpawnX", world.SpawnX));
                    break;
                case WorldSave.SpawnY:
                    tag1.Add(new IntTag("SpawnY", world.SpawnY));
                    break;
                case WorldSave.SpawnZ:
                    tag1.Add(new IntTag("SpawnZ", world.SpawnZ));
                    break;
                case WorldSave.LastPlayed:
                    tag1.Add(new LongTag("LastPlayed", world.LastPlayed));
                    break;
                case WorldSave.GameType:
                    tag1.Add(new IntTag("GameType", world.GameType));
                    break;
                case WorldSave.MapFeatures:
                    tag1.Add(new ByteTag("MapFeatures", world.MapFeatures));
                    break;
                case WorldSave.ThunderTime:
                    tag1.Add(new IntTag("thunderTime", world.ThunderTime));
                    break;
                case WorldSave.Version:
                    tag1.Add(new IntTag("version", world.Version));
                    break;
                case WorldSave.RainTime:
                    tag1.Add(new IntTag("rainTime", world.RainTime));
                    break;
                case WorldSave.Time:
                    tag1.Add(new LongTag("Time", world.Time));
                    break;
                case WorldSave.Thundering:
                    tag1.Add(new ByteTag("thundering", world.Thundering));
                    break;
                case WorldSave.Hardcore:
                    tag1.Add(new ByteTag("hardcore", world.Hardcore));
                    break;
                case WorldSave.SizeOnDisk:
                    tag1.Add(new LongTag("SizeOnDisk", world.SizeOnDisk));
                    break;
                case WorldSave.LevelName:
                    tag1.Add(new StringTag("LevelName", world.LevelName));
                    break;
            }
        }

        tag.Add(tag1);
        await NbtFile.WriteAsync(world.Local + "/level.dat", tag, FormatOptions.Java, CompressionType.AutoDetect);
    }
}
