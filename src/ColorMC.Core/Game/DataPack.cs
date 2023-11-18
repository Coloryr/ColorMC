using ColorMC.Core.Helpers;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ColorMC.Core.Game;

/// <summary>
/// 数据包处理
/// </summary>
public static class DataPack
{
    /// <summary>
    /// 检查数据包
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="ens">已启用的数据包</param>
    /// <param name="dis">已禁用的数据包</param>
    /// <param name="data">数据包内容</param>
    /// <returns></returns>
    private static DataPackObj? CheckPack(string path, NbtList? ens, NbtList? dis, JObject data)
    {
        if (data.TryGetValue("pack", out var obj) && obj is JObject obj1)
        {
            var item1 = new DataPackObj()
            {
                Name = "file/" + Path.GetFileName(path),
                Path = path,
                Description = obj1["description"]?.ToString() ?? "",
                PackFormat = obj1["pack_format"]?.Value<int>() ?? -1
            };

            if (ens != null)
            {
                foreach (var item2 in ens)
                {
                    if (item2 is NbtString item3 && item3.Value == item1.Name)
                    {
                        item1.Enable = true;
                        break;
                    }
                }
            }

            if (dis != null)
            {
                foreach (var item2 in dis)
                {
                    if (item2 is NbtString item3 && item3.Value == item1.Name)
                    {
                        item1.Enable = false;
                        break;
                    }
                }
            }

            return item1;
        }

        return null;
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    /// <param name="world">世界存储</param>
    /// <returns>列表</returns>
    public static List<DataPackObj> GetDataPacks(this WorldObj world)
    {
        var list = new List<DataPackObj>();

        var path = world.GetWorldDataPacksPath();
        if (!Directory.Exists(path))
        {
            return list;
        }

        var nbt = (world.Nbt.TryGet("Data") as NbtCompound)?.TryGet("DataPacks") as NbtCompound;
        var ens = nbt?.TryGet("Enabled") as NbtList;
        var dis = nbt?.TryGet("Disabled") as NbtList;

        var files = Directory.GetFiles(path);
        foreach (var item in files)
        {
            if (item.EndsWith(".zip"))
            {
                try
                {
                    using var file = PathHelper.OpenRead(item)!;
                    using var zip = new ZipFile(file);
                    var ent = zip.GetEntry("pack.mcmeta");
                    if (ent == null)
                    {
                        continue;
                    }
                    using var stream = new MemoryStream();
                    using var stream1 = zip.GetInputStream(ent);
                    stream1.CopyTo(stream);
                    var data = JObject.Parse(Encoding.UTF8.GetString(stream.ToArray()));
                    var pack = CheckPack(item, ens, dis, data);
                    if (pack != null)
                    {
                        pack.World = world;
                        list.Add(pack);
                    }
                }
                catch (Exception e)
                {
                    Logs.Error(string.Format(LanguageHelper.Get("Core.DataPack.Error1"), item), e);
                }
            }
        }

        var paths = Directory.GetDirectories(path);
        foreach (var item in paths)
        {
            try
            {
                var file = Path.GetFullPath(item + "/pack.mcmeta");
                if (!File.Exists(file))
                {
                    continue;
                }
                var str = PathHelper.ReadText(file)!;
                var data = JObject.Parse(str);
                var pack = CheckPack(item, ens, dis, data);
                if (pack != null)
                {
                    pack.World = world;
                    list.Add(pack);
                }
            }
            catch (Exception e)
            {
                Logs.Error(string.Format(LanguageHelper.Get("Core.DataPack.Error1"), item), e);
            }
        }

        return list;
    }

    /// <summary>
    /// 禁用启用数据包
    /// </summary>
    /// <param name="list">数据包列表</param>
    /// <param name="world">世界储存</param>
    /// <returns></returns>
    public static bool DisE(IEnumerable<DataPackObj> list, WorldObj world)
    {
        var nbt = (world.Nbt.TryGet("Data") as NbtCompound)?.TryGet("DataPacks") as NbtCompound;

        if (nbt?.TryGet("Enabled") is not NbtList ens
            || nbt?.TryGet("Disabled") is not NbtList dis)
        {
            return false;
        }

        bool enable = false;
        bool disable = false;

        NbtString? nbten = null;
        NbtString? nbtdi = null;

        foreach (var obj in list)
        {
            foreach (var item in ens)
            {
                if (item is NbtString str)
                {
                    if (str.Value == obj.Name)
                    {
                        nbten = str;
                        enable = true;
                        break;
                    }
                }
            }
            foreach (var item in dis)
            {
                if (item is NbtString str)
                {
                    if (str.Value == obj.Name)
                    {
                        nbtdi = str;
                        disable = true;
                        break;
                    }
                }
            }

            if (enable && disable)
            {
                dis.Remove(nbtdi!);
            }
            else if (enable)
            {
                ens.Remove(nbten!);
                dis.Add(nbten!);
            }
            else if (disable)
            {
                dis.Remove(nbtdi!);
                ens.Add(nbtdi!);
            }
            else
            {
                ens.Add(new NbtString() { Value = obj.Name });
            }
        }

        world.Nbt.Save(Path.GetFullPath(world.Local + "/level.dat"));

        return true;
    }

    /// <summary>
    /// 删除数据包
    /// </summary>
    /// <param name="list">数据包列表</param>
    /// <param name="world">世界存储</param>
    /// <returns>是否删除</returns>
    public static async Task<bool> Delete(List<DataPackObj> list, WorldObj world)
    {
        var nbt = (world.Nbt.TryGet("Data") as NbtCompound)?.TryGet("DataPacks") as NbtCompound;

        if (nbt?.TryGet("Enabled") is not NbtList ens
            || nbt?.TryGet("Disabled") is not NbtList dis)
        {
            return false;
        }

        foreach (var item in list)
        {
            if (Directory.Exists(item.Path))
            {
                await PathHelper.DeleteFiles(item.Path);
            }
            else
            {
                File.Delete(item.Path);
            }
        }

        await Task.Run(() =>
        {
            try
            {
                foreach (var obj in list)
                {
                    foreach (var item in ens.CopyList())
                    {
                        if (item is NbtString str && str.Value == obj.Name)
                        {
                            ens.Remove(item);
                        }
                    }

                    foreach (var item in dis.CopyList())
                    {
                        if (item is NbtString str && str.Value == obj.Name)
                        {
                            ens.Remove(item);
                        }
                    }
                }

                world.Nbt.Save(Path.GetFullPath(world.Local + "/level.dat"));
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.DataPack.Error2"), e);
            }
        });

        return true;
    }
}
