using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ColorMC.Core.Game;

/// <summary>
/// 材质包相关操作
/// </summary>
public static class Resourcepacks
{
    /// <summary>
    /// 获取材质包列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>列表</returns>
    public static async Task<List<ResourcepackObj>> GetResourcepacks(this GameSettingObj game)
    {
        var list = new List<ResourcepackObj>();
        var dir = game.GetResourcepacksPath();

        DirectoryInfo info = new(dir);
        if (!info.Exists)
        {
            info.Create();
            return list;
        }

        await Parallel.ForEachAsync(info.GetFiles(), async (item, cancel) =>
        {
            string sha1;
            {
                using var stream = File.OpenRead(item.FullName);
                sha1 = Funtcions.GenSha1(stream);
            }
            bool find = false;
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
                    var obj1 = JObject.Parse(data);
                    if (obj1 != null)
                    {
                        ResourcepackObj obj = new()
                        {
                            Local = Path.GetFullPath(item.FullName),
                            Sha1 = sha1,
                        };
                        if (obj1.ContainsKey("pack"))
                        {
                            var obj2 = (obj1["pack"] as JObject)!;
                            if (obj2.TryGetValue("pack_format", out var item2))
                            {
                                obj.pack_format = (int)item2;
                            }
                            if (obj2.ContainsKey("description"))
                            {
                                var obj3 = obj2["description"]!;
                                if (obj3.Type == JTokenType.String)
                                {
                                    obj.description = obj3.ToString();
                                }
                                else if (obj3.Type == JTokenType.Object)
                                {
                                    obj.description = obj3["fallback"]?.ToString() ?? "";
                                }
                            }
                        }
                        item1 = zFile.GetEntry("pack.png");
                        if (item1 != null)
                        {
                            using var stream2 = zFile.GetInputStream(item1);
                            using var stream3 = new MemoryStream();
                            await stream2.CopyToAsync(stream3, cancel);
                            obj.Icon = stream3.ToArray();
                        }
                        list.Add(obj);
                        find = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.GetName("Core.Game.Error2"), e);
            }

            if (!find)
            {
                list.Add(new()
                {
                    Sha1 = sha1,
                    Local = Path.GetFullPath(item.FullName),
                    Broken = true
                });
            }
        });

        return list;
    }

    /// <summary>
    /// 导入材质包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">导入列表</param>
    /// <returns>结果</returns>
    public static async Task<bool> AddResourcepack(this GameSettingObj obj, List<string> file)
    {
        var path = obj.GetResourcepacksPath();
        bool ok = true;
        foreach (var item in file)
        {
            var name = Path.GetFileName(item);
            var local = Path.GetFullPath(path + "/" + name);
            if (File.Exists(local))
                return false;

            await Task.Run(() =>
            {
                try
                {
                    File.Copy(item, local);
                }
                catch (Exception e)
                {
                    Logs.Error(LanguageHelper.GetName("Core.Game.Error3"), e);
                    ok = false;
                }
            });
            if (!ok)
                return false;
        };
        return ok;
    }
}
