using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;

namespace ColorMC.Core.Helpers;

/// <summary>
/// Modrinth处理
/// </summary>
public static class ModrinthHelper
{
    private static List<ModrinthCategoriesObj>? s_categories;
    private static List<string>? s_gameVersions;

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj MakeModDownloadObj(this ModrinthVersionObj data, GameSettingObj obj)
    {
        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        return new DownloadItemObj()
        {
            Name = data.name,
            Url = file.url,
            Local = Path.GetFullPath(obj.GetModsPath() + "/" + file.filename),
            SHA1 = file.hashes.sha1
        };
    }

    /// <summary>
    /// 创建下载项目
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="obj">游戏实例</param>
    /// <returns>下载项目</returns>
    public static DownloadItemObj MakeDownloadObj(this ModrinthPackObj.File data, GameSettingObj obj)
    {
        return new DownloadItemObj()
        {
            Url = data.downloads[0],
            Name = data.path,
            Local = obj.GetGamePath() + "/" + data.path,
            SHA1 = data.hashes.sha1
        };
    }

    /// <summary>
    /// 创建Mod信息
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns>Mod信息</returns>
    public static ModInfoObj MakeModInfo(this ModrinthVersionObj data)
    {
        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        return new ModInfoObj()
        {
            Path = "mods",
            FileId = data.id.ToString(),
            ModId = data.project_id,
            File = file.filename,
            Name = data.name,
            Url = file.url,
            SHA1 = file.hashes.sha1
        };
    }

    /// <summary>
    /// 获取MO分组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string>?> GetModrinthCategories(FileType type)
    {
        if (s_categories == null)
        {
            var list6 = await ModrinthAPI.GetCategories();
            if (list6 == null)
            {
                return null;
            }

            s_categories = list6;
        }

        var list7 = from item2 in s_categories
                    where item2.project_type == type switch
                    {
                        FileType.Shaderpack => ModrinthAPI.ClassShaderpack,
                        FileType.Resourcepack => ModrinthAPI.ClassResourcepack,
                        FileType.ModPack => ModrinthAPI.ClassModPack,
                        _ => ModrinthAPI.ClassMod
                    }
                    && item2.header == "categories"
                    orderby item2.name descending
                    select item2.name;

        return list7.ToDictionary(a => a);
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    public static async Task<List<string>?> GetGameVersion()
    {
        if (s_gameVersions != null)
        {
            return s_gameVersions;
        }

        var list = await ModrinthAPI.GetGameVersions();
        if (list == null)
        {
            return null;
        }

        var list1 = new List<string>
        {
            ""
        };

        list1.AddRange(from item in list select item.version);

        s_gameVersions = list1;

        return list1;
    }
}
