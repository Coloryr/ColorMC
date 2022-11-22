using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace ColorMC.Core.Game;

public static class Resourcepacks
{
    public static async Task<List<ResourcepackObj>> GetResourcepacks(this GameSettingObj game)
    {
        var list = new List<ResourcepackObj>();
        var dir = game.GetResourcepacksPath();
        
        DirectoryInfo info = new(dir);
        if (!info.Exists)
            return list;

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = 10
        };
        await Parallel.ForEachAsync(info.GetFiles(), options, async (item, cancel) =>
        {
            if (item.Extension is not (".zip" or ".disable"))
                return;
            try
            {
                using ZipFile zFile = new(item.FullName);
                var item1 = zFile.GetEntry("pack.mcmeta");
                if (item1 != null)
                {
                    using var stream1 = zFile.GetInputStream(item1);
                    using var stream = new MemoryStream();
                    await stream1.CopyToAsync(stream, cancel);
                    var data = Encoding.UTF8.GetString(stream.ToArray());
                    var obj1 = JsonConvert.DeserializeObject<ResourcepackObj>(data);
                    if (obj1 != null)
                    {
                        obj1.Disable = item.Extension is ".disable";
                        obj1.Local = Path.GetFullPath(item.FullName);
                        item1 = zFile.GetEntry("pack.png");
                        if (item1 != null)
                        {
                            using var stream2 = zFile.GetInputStream(item1);
                            using var stream3 = new MemoryStream();
                            await stream2.CopyToAsync(stream3, cancel);
                            obj1.Icon = stream3.ToArray();
                        }
                        list.Add(obj1);
                    }
                }
            }
            catch (Exception e)
            {
                Logs.Error("Mod解析失败", e);
            }
        });

        return list;
    }

    public static void Disable(this ResourcepackObj pack)
    {
        if (pack.Disable)
            return;

        var file = new FileInfo(pack.Local);
        pack.Disable = true;
        pack.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name.Replace(".zip", ".disable")}");
        File.Move(file.FullName, pack.Local);
    }

    public static void Enable(this ResourcepackObj pack)
    {
        if (!pack.Disable)
            return;

        var file = new FileInfo(pack.Local);
        pack.Disable = false;
        pack.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name.Replace(".disable", ".zip")}");
        File.Move(file.FullName, pack.Local);
    }
}
