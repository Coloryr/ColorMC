using System.Text;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;

namespace ColorMC.Core.Helpers;

/// <summary>
/// CueseForge处理
/// </summary>
public static class CurseForgeHelper
{
    private static CurseForgeCategoriesObj? s_categories;
    private static List<string>? s_supportVersion;
    /// <summary>
    /// 修正下载地址
    /// </summary>
    /// <param name="item"></param>
    public static void FixDownloadUrl(this CurseForgeModObj.Data item)
    {
        item.downloadUrl ??= $"{UrlHelper.CurseForgeDownload}files/{item.id / 1000}/{item.id % 1000}/{item.fileName}";
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">CurseForge数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj MakeModDownloadObj(this CurseForgeModObj.Data data, GameSettingObj obj, string path)
    {
        data.FixDownloadUrl();

        return new DownloadItemObj()
        {
            Url = data.downloadUrl,
            Name = data.displayName,
            Local = path + "/" + data.fileName,
            SHA1 = data.hashes.Where(a => a.algo == 1)
                    .Select(a => a.value).FirstOrDefault()
        };
    }

    /// <summary>
    /// 创建Mod信息
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Mod信息</returns>
    public static ModInfoObj MakeModInfo(this CurseForgeModObj.Data data, string path)
    {
        data.FixDownloadUrl();

        return new ModInfoObj()
        {
            Path = path,
            Name = data.displayName,
            File = data.fileName,
            SHA1 = data.hashes.Where(a => a.algo == 1)
                    .Select(a => a.value).FirstOrDefault(),
            ModId = data.modId.ToString(),
            FileId = data.id.ToString(),
            Url = data.downloadUrl
        };
    }

    /// <summary>
    /// 获取作者名字
    /// </summary>
    /// <param name="authors"></param>
    /// <returns>作者名字</returns>
    public static string GetString(this List<CurseForgeObjList.Data.Authors> authors)
    {
        if (authors == null || authors.Count == 0)
        {
            return "";
        }
        var builder = new StringBuilder();
        foreach (var item in authors)
        {
            builder.Append(item.name).Append(',');
        }

        return builder.ToString()[..^1];
    }

    /// <summary>
    /// 获取CF分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string>?> GetCurseForgeCategories(FileType type)
    {
        if (s_categories == null)
        {
            var list6 = await CurseForgeAPI.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            s_categories = list6;
        }

        var list7 = from item2 in s_categories.data
                    where item2.classId == type switch
                    {
                        FileType.Mod => CurseForgeAPI.ClassMod,
                        FileType.World => CurseForgeAPI.ClassWorld,
                        FileType.Resourcepack => CurseForgeAPI.ClassResourcepack,
                        FileType.Shaderpack => CurseForgeAPI.ClassShaderpack,
                        _ => CurseForgeAPI.ClassModPack
                    }
                    orderby item2.name descending
                    select (item2.name, item2.id);

        return list7.ToDictionary(a => a.id.ToString(), a => a.name);
    }

    /// <summary>
    /// 获取CurseForge支持的游戏版本
    /// </summary>
    /// <returns>游戏版本</returns>
    public static async Task<List<string>?> GetGameVersions()
    {
        if (s_supportVersion != null)
        {
            return s_supportVersion;
        }
        var list = await CurseForgeAPI.GetCurseForgeVersionType();
        if (list == null)
        {
            return null;
        }

        list.data.RemoveAll(a =>
        {
            return !a.name.StartsWith("Minecraft ");
        });

        var list111 = new List<CurseForgeVersionType.Item>();
        list111.AddRange(from item in list.data
                         where item.id > 17
                         orderby item.id descending
                         select item);
        list111.AddRange(from item in list.data
                         where item.id < 18
                         orderby item.id ascending
                         select item);

        var list2 = await CurseForgeAPI.GetCurseForgeVersion();
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

        s_supportVersion = list3;

        return list3;
    }
}
