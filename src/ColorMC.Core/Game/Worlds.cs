using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using fNbt;
using ICSharpCode.SharpZipLib.Zip;

namespace ColorMC.Core.Game;

public static class Worlds
{

    /// <summary>
    /// 获取世界列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>世界列表</returns>
    public static async Task<List<WorldObj>> GetWorlds(this GameSettingObj game)
    {
        List<WorldObj> list = new();
        string dir = game.GetSavesPath();

        DirectoryInfo info = new(dir);
        if (!info.Exists)
            return list;

        await Parallel.ForEachAsync(info.GetDirectories(), (item, cacenl) =>
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
                    Logs.Error(LanguageHelper.GetName("Core.Game.Error4"), e);
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

    /// <summary>
    /// 删除世界
    /// </summary>
    /// <param name="world">世界实例</param>
    public static void Remove(this WorldObj world)
    {
        string dir = Path.GetFullPath(world.Game.GetBasePath() + "/remove_worlds");
        Directory.CreateDirectory(dir);
        Directory.Move(world.Local, Path.GetFullPath(dir + world.LevelName));
    }

    /// <summary>
    /// 导入世界压缩包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件位置</param>
    /// <returns>结果</returns>
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
            Logs.Error(LanguageHelper.GetName("Core.Path.Instances.Load.Error"), e);
        }

        return false;
    }

    /// <summary>
    /// 导出世界
    /// </summary>
    /// <param name="world">世界实例</param>
    /// <param name="file">输出文件位置</param>
    /// <returns></returns>
    public static Task ExportWorldZip(this WorldObj world, string file)
    {
        return ZipFloClass.ZipFile(world.Local, file);
    }
}
