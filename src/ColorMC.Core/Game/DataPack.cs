using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using ColorMC.Core.Helpers;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 数据包处理
/// </summary>
public static class DataPack
{
    /// <summary>
    /// 获取数据包列表
    /// </summary>
    /// <param name="world">世界存储</param>
    /// <returns>数据包列表</returns>
    public static Task<List<DataPackObj>> GetDataPacksAsync(this WorldObj world)
    {
        return Task.Run(world.GetDataPacks);
    }

    /// <summary>
    /// 获取数据包列表
    /// </summary>
    /// <param name="world">世界存储</param>
    /// <returns>数据包列表</returns>
    public static List<DataPackObj> GetDataPacks(this WorldObj world)
    {
        var path = world.GetWorldDataPacksPath();
        if (!Directory.Exists(path))
        {
            return [];
        }

        var list = new List<DataPackObj>();

        //查找数据包的NBT
        var nbt = world.Nbt.TryGet<NbtCompound>("Data")?.TryGet<NbtCompound>("DataPacks");
        if (nbt == null)
        {
            return list;
        }
        var ens = nbt?.TryGet<NbtList>("Enabled");
        var dis = nbt?.TryGet<NbtList>("Disabled");

        //从压缩包读取数据包
        var files = Directory.GetFiles(path);
#if false
        Parallel.ForEach(files, new()
        {
            MaxDegreeOfParallelism = 1
        }, (item) =>
#else
        Parallel.ForEach(files, async (item) =>
#endif
        {
            if (!item.EndsWith(".zip"))
            {
                return;
            }

            try
            {
                using var zip = ZipFile.OpenRead(item);
                var ent = zip.GetEntry(Names.NamePackMetaFile);
                if (ent == null)
                {
                    return;
                }
                using var stream = ent.Open();
                var data = await JsonUtils.ReadAsObjAsync(stream);
                if (data == null)
                {
                    return;
                }
                //检查数据包是否正确
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
        });

        //从文件夹读取数据包
        var paths = Directory.GetDirectories(path);
#if false
        Parallel.ForEach(paths, new()
        {
            MaxDegreeOfParallelism = 1
        }, (item) =>
#else
        Parallel.ForEach(paths, async (item) =>
#endif
        {
            try
            {
                var file = Path.Combine(item, Names.NamePackMetaFile);
                using var str = PathHelper.OpenRead(file);
                var data = await JsonUtils.ReadAsObjAsync(str);
                if (data == null)
                {
                    return;
                }
                //检查数据包是否正确
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
        });

        list.Sort(DataPackObjComparer.Instance);

        return list;
    }

    /// <summary>
    /// 禁用/启用世界数据包
    /// </summary>
    /// <param name="world">世界储存</param>
    /// <param name="list">数据包列表</param>
    /// <returns>是否成功设置</returns>
    public static bool DisableOrEnableDataPack(this WorldObj world, IEnumerable<DataPackObj> list)
    {
        var nbt = world.Nbt.TryGet<NbtCompound>("Data")?.TryGet<NbtCompound>("DataPacks");

        if (nbt?.TryGet<NbtList>("Enabled") is not { } ens
            || nbt?.TryGet<NbtList>("Disabled") is not { } dis)
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
                if (item is NbtString str && str.Value == obj.Name)
                {
                    nbten = str;
                    enable = true;
                    break;
                }
            }
            foreach (var item in dis)
            {
                if (item is NbtString str && str.Value == obj.Name)
                {
                    nbtdi = str;
                    disable = true;
                    break;
                }
            }

            if (enable && disable)
            {
                //启用
                dis.Remove(nbtdi!);
            }
            else if (enable)
            {
                //禁用
                ens.Remove(nbten!);
                dis.Add(nbten!);
            }
            else if (disable)
            {
                //启用
                dis.Remove(nbtdi!);
                ens.Add(nbtdi!);
            }
            else
            {
                //启用
                ens.Add(new NbtString() { Value = obj.Name });
            }
        }

        world.Nbt.Save(Path.Combine(world.Local, Names.NameLevelFile));

        return true;
    }

    /// <summary>
    /// 删除世界数据包
    /// </summary>
    /// <param name="world">世界存储</param>
    /// <param name="arg">删除参数</param>
    /// <returns>是否删除成功</returns>
    public static async Task<bool> DeleteDataPackAsync(this WorldObj world, DataPackDeleteArg arg)
    {
        var nbt = world.Nbt.TryGet<NbtCompound>("Data")?.TryGet<NbtCompound>("DataPacks");

        if (nbt?.TryGet<NbtList>("Enabled") is not { } ens
            || nbt?.TryGet<NbtList>("Disabled") is not { } dis)
        {
            return false;
        }

        foreach (var item in arg.List)
        {
            if (Directory.Exists(item.Path))
            {
                await PathHelper.DeleteFilesAsync(new DeleteFilesArg
                {
                    Local = item.Path,
                    Request = arg.Request
                });
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
                //删除存在的
                foreach (var obj in arg.List)
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

                world.Nbt.Save(Path.Combine(world.Local, Names.NameLevelFile));
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.DataPack.Error2"), e);
            }
        });

        return true;
    }

    /// <summary>
    /// 检查数据包
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="ens">已启用的数据包</param>
    /// <param name="dis">已禁用的数据包</param>
    /// <param name="data">数据包内容</param>
    /// <returns>数据包信息</returns>
    private static DataPackObj? CheckPack(string path, NbtList? ens, NbtList? dis, JsonObject data)
    {
        if (data.GetObj("pack") is { } obj)
        {
            var item1 = new DataPackObj()
            {
                Name = "file/" + Path.GetFileName(path),
                Path = path,
                Description = obj.GetString("description") ?? "",
                PackFormat = obj.GetInt("pack_format") ?? -1
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
}
