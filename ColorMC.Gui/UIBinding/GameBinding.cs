using ColorMC.Core.Game.Auth;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Utils;
using System.Collections;
using DynamicData;
using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core;

namespace ColorMC.Gui.UIBinding;

public static class GameBinding
{
    private readonly static List<string> SortOrder = new() { "顺序", "倒序" };
    public static List<GameSettingObj> GetGames()
    {
        return InstancesPath.Games;
    }

    public static List<string> GetGameVersion(bool? type1, bool? type2, bool? type3)
    {
        var list = new List<string>();
        if (VersionPath.Versions == null)
            return list;

        foreach (var item in VersionPath.Versions.versions)
        {
            if (item.type == "release")
            {
                if (type1 == true)
                {
                    list.Add(item.id);
                }
            }
            else if (item.type == "snapshot")
            {
                if (type2 == true)
                {
                    list.Add(item.id);
                }
            }
            else
            {
                if (type3 == true)
                {
                    list.Add(item.id);
                }
            }
        }

        return list;
    }

    public static async Task<bool> AddGame(string name, string version,
        Loaders loaders, string? loaderversion = null)
    {
        var game = new GameSettingObj()
        {
            Name = name,
            Version = version,
            Loader = loaders,
            LoaderVersion = loaderversion
        };

        game = await InstancesPath.CreateVersion(game);

        return game != null;
    }

    public static Task<bool> AddPack(string dir, PackType type)
    {
        return InstancesPath.InstallFromZip(dir, type);
    }

    public static Dictionary<string, List<GameSettingObj>> GetGameGroups()
    {
        return InstancesPath.Groups;
    }

    public static Task<CurseForgeObj?> GetPackList(string version, int sort, string filter, int page, int sortOrder)
    {
        return CurseForge.GetPackList(version, page, sort, filter, sortOrder: sortOrder);
    }

    public static List<string> GetCurseForgeTypes()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(SortField));
        foreach (SortField value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }

    public static List<string> GetSortOrder()
    {
        return SortOrder;
    }

    public static async Task<List<string>?> GetCurseForgeGameVersions()
    {
        var list = await CurseForge.GetCurseForgeVersionType();
        if (list == null)
        {
            return null;
        }

        list.data.RemoveAll(a =>
        {
            return a.id is 68441 or 615 or 1 or 3 or 2 or 73247 or 75208;
        });

        var list1 = from item in list.data
                    where item.id > 17
                    orderby item.id descending 
                    select item;

        var list11 = from item in list.data
                    where item.id < 18
                    orderby item.id ascending
                    select item;

        var list111 = new List<CurseForgeVersionType.Item>();
        list111.AddRange(list1);
        list111.AddRange(list11);

        var list2 = await CurseForge.GetCurseForgeVersion();
        if (list2 == null)
        {
            return null;
        }

        var list3 = new List<string>
        {
            ""
        };
        foreach (var item in list111)
        {
            var list4 = from item1 in list2.data
                        where item1.type == item.id
                        select item1.versions;
            var list5 = list4.FirstOrDefault();
            if (list5 != null)
            {
                list3.AddRange(list5);
            }
        }

        return list3;
    }

    public static Task<bool> InstallCurseForge(CurseForgeObj.Data.LatestFiles data)
    {
        return InstancesPath.InstallFromCurseForge(data);
    }

    public static Task<CurseForgeFileObj?> GetPackFile(long id, int page)
    {
        return CurseForge.GetCurseForgeFiles(id, page);
    }
}
