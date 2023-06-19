using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using System.Text;

namespace ColorMC.Core.Helpers;

/// <summary>
/// CueseForge
/// </summary>
public static class CurseForgeHelper
{
    /// <summary>
    /// 修正下载地址
    /// </summary>
    /// <param name="item"></param>
    public static void FixDownloadUrl(this CurseForgeObjList.Data.LatestFiles item)
    {
        item.downloadUrl ??= $"{UrlHelper.CurseForgeDownload}files/{item.id / 1000}/{item.id % 1000}/{item.fileName}";
    }
    /// <summary>
    /// 创建下载地址
    /// </summary>
    /// <param name="data"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static DownloadItemObj MakeModDownloadObj(this CurseForgeObjList.Data.LatestFiles data, GameSettingObj obj)
    {
        data.FixDownloadUrl();

        return new DownloadItemObj()
        {
            Url = data.downloadUrl,
            Name = data.displayName,
            Local = obj.GetModsPath() + "/" + data.fileName,
            SHA1 = data.hashes.Where(a => a.algo == 1)
                    .Select(a => a.value).FirstOrDefault(),
            Overwrite = true
        };
    }

    /// <summary>
    /// 创建Mod信息
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ModInfoObj MakeModInfo(this CurseForgeObjList.Data.LatestFiles data)
    {
        data.FixDownloadUrl();

        return new ModInfoObj()
        {
            Path = "mods",
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
    /// <returns></returns>
    public static string GetString(this List<CurseForgeObjList.Data.Authors> authors)
    {
        if (authors == null || authors.Count == 0)
            return "";
        var builder = new StringBuilder();
        foreach (var item in authors)
        {
            builder.Append(item.name).Append(',');
        }

        return builder.ToString()[..^1];
    }
}
