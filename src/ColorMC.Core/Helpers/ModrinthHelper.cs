using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;

namespace ColorMC.Core.Helpers;

public static class ModrinthHelper
{
    /// <summary>
    /// 创建下载地址
    /// </summary>
    /// <param name="data"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
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
    /// <param name="data"></param>
    /// <returns></returns>
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
}
