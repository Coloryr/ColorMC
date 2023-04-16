using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Modrinth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

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
            SHA1 = file.hashes.sha1,
            Overwrite = true
        };
    }

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
