using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using Esprima.Ast;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Apis;

public static class Modrinth
{
    public const string Url = "https://api.modrinth.com/v2/";

    public static async Task<ModrinthSearchObj?> Search(string query, MSortingObj type, List<MFacetsObj>? type1, int offset, int limit)
    {
        try
        {
            string url = $"{Url}search?query={query}&index={type.Data}&offset={offset}&limit={limit}";
            if (type1 != null && type1.Count > 0)
            {
                url += $"&facets={MFacetsObj.Build(type1)}";
            }
            var res = await BaseClient.DownloadClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ModrinthSearchObj>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    /// <summary>
    /// 获取整合包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetModPackList(string version = "", 
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        var list = new List<MFacetsObj>
        {
            MFacetsObj.BuildProjectType(new() { "modpack" })
        };
        if (!string.IsNullOrWhiteSpace(version))
        {
            list.Add(MFacetsObj.BuildVersions(new() { version }));
        }
        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            list.Add(MFacetsObj.BuildCategories(new() { categoryId }));
        }
        MSortingObj type = sortOrder switch
        { 
            1 => MSortingObj.Downloads,
            2 => MSortingObj.Follows,
            3 => MSortingObj.Newest,
            4 => MSortingObj.Updated,
            _ => MSortingObj.Relevance
        };


        return Search(filter, type, list, page * pagesize, pagesize);
    }

    /// <summary>
    /// 获取Mod列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetModList(string version = "",
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        var list = new List<MFacetsObj>
        {
            MFacetsObj.BuildProjectType(new() { "mod" })
        };
        if (!string.IsNullOrWhiteSpace(version))
        {
            list.Add(MFacetsObj.BuildVersions(new() { version }));
        }
        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            list.Add(MFacetsObj.BuildCategories(new() { categoryId }));
        }
        MSortingObj type = sortOrder switch
        {
            1 => MSortingObj.Downloads,
            2 => MSortingObj.Follows,
            3 => MSortingObj.Newest,
            4 => MSortingObj.Updated,
            _ => MSortingObj.Relevance
        };

        return Search(filter, type, list, page * pagesize, pagesize);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    public static Task<ModrinthSearchObj?> GetResourcepackList(string version = "",
        int page = 0, string filter = "", int pagesize = 50, int sortOrder = 0,
        string categoryId = "")
    {
        var list = new List<MFacetsObj>
        {
            MFacetsObj.BuildProjectType(new() { "resourcepack" })
        };
        if (!string.IsNullOrWhiteSpace(version))
        {
            list.Add(MFacetsObj.BuildVersions(new() { version }));
        }
        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            list.Add(MFacetsObj.BuildCategories(new() { categoryId }));
        }
        MSortingObj type = sortOrder switch
        {
            1 => MSortingObj.Downloads,
            2 => MSortingObj.Follows,
            3 => MSortingObj.Newest,
            4 => MSortingObj.Updated,
            _ => MSortingObj.Relevance
        };

        return Search(filter, type, list, page * pagesize, pagesize);
    }

    public static async Task<ModrinthProjectObj?> Project(string id)
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}project/{id}");
            return JsonConvert.DeserializeObject<ModrinthProjectObj>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    public static async Task<List<ModrinthVersionObj>?> Version(string id)
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}project/{id}/version");
            return JsonConvert.DeserializeObject<List<ModrinthVersionObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    public static async Task<List<ModrinthGameVersionObj>?> GetGameVersion()
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}tag/game_version");
            return JsonConvert.DeserializeObject<List<ModrinthGameVersionObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }

    public static async Task<List<ModrinthCategoriesObj>?> GetCategories()
    {
        try
        {
            var res = await BaseClient.DownloadClient.GetStringAsync($"{Url}tag/category");
            return JsonConvert.DeserializeObject<List<ModrinthCategoriesObj>>(res);
        }
        catch (Exception e)
        {
            Logs.Error("get fail", e);
            return null;
        }
    }
}
