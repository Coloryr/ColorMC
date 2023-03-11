using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Game;

public static class ServerPack
{
    public static void MoveToOld(this ServerPackObj obj)
    {
        var file1 = obj.Game.GetServerPackOldFile();
        var file2 = obj.Game.GetServerPackFile();

        if (File.Exists(file1))
        {
            File.Delete(file1);
        }

        File.Move(file2, file1);
    }

    public static async Task<bool> Update(this ServerPackObj obj)
    {
        File.Delete(obj.Game.GetServerPackFile());
        var old = obj.Game.GetOldServerPack();

        var list5 = new List<DownloadItem>();

        var path = obj.Game.GetModsPath();

        if (old == null)
        {
            old = new()
            {
                Mod = new()
            };
        }

        var list1 = obj.Mod.ToArray();
        var list2 = old.Mod.ToArray();

        for (int a = 0; a < list2.Length; a++)
        {
            var item1 = list1[a];
            for (int b = 0; b < list2.Length; b++)
            {
                var item2 = list2[a];
                if (item2 == null)
                    continue;
                if (item2.Sha1 == item1.Sha1 || item2.File == item1.File)
                {
                    list1[a] = null;
                    list2[a] = null;
                    break;
                }
            }
        }

        var list3 = list1.ToList();
        var list4 = list2.ToList();
        list3.RemoveAll(a => a == null);
        list4.RemoveAll(a => a == null);

        foreach (var item in list3)
        {
            if (item.Source == null)
            {
                list5.Add(new()
                {
                    Name = item.File,
                    Local = Path.GetFullPath(path + item.File),
                    SHA1 = item.Sha1,
                    Url = item.Url
                });
            }
            else
            {
                list5.Add(new()
                {
                    Name = item.File,
                    Local = Path.GetFullPath(path + item.File),
                    SHA1 = item.Sha1,
                    Url = UrlHelper.MakeDownloadUrl(item.Source, item.Projcet, item.FileId, item.File)
                });
            }
        }

        var mods = await obj.Game.GetMods();

        foreach (var item in list4)
        {
            mods.Find(a => a.Sha1 == item.Sha1)?.Delete();
        }

        path = obj.Game.GetResourcepacksPath();

        foreach (var item in obj.Resourcepack)
        {
            list5.Add(new()
            {
                Name = item.File,
                Local = Path.GetFullPath(path + item.File),
                SHA1 = item.Sha1,
                Url = item.Url
            });
        }

        path = obj.Game.GetGamePath();
        var path1 = obj.Game.GetBasePath();

        foreach (var item in obj.Config)
        {
            if (item.Zip)
            {
                list5.Add(new()
                {
                    Name = item.FileName,
                    Local = Path.GetFullPath(path1 + "/" + item.FileName),
                    SHA1 = item.Sha1,
                    Url = item.Url,
                    Later = (stream) =>
                    {
                        if (item.Dir)
                        {
                            ZipUtils.Unzip(Path.GetFullPath(path + "/" + item.Group), stream, false);
                        }
                    }
                });
            }
            else
            {
                list5.Add(new()
                {
                    Name = item.Group + item.FileName,
                    Local = Path.GetFullPath(path + "/" + item.Group + item.FileName),
                    SHA1 = item.Sha1,
                    Url = item.Url
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(obj.UI))
        {
            list5.Add(new()
            {
                Name = obj.UI,
                Local = Path.GetFullPath(ColorMCCore.BaseDir +  obj.UI),
                Url = obj.Url + obj.UI
            });
        }

        var res = await DownloadManager.Start(list5);
        if (!res)
            return false;

        obj.Game.Save();
        obj.Save();

        return true;
    }

    public static ServerPackObj GetServerPack(this GameSettingObj obj)
    {
        var file = obj.GetServerPackFile();
        ServerPackObj? obj1 = null;
        if (File.Exists(file))
        {
            var data = File.ReadAllText(file);
            obj1 = JsonConvert.DeserializeObject<ServerPackObj>(data);
        }

        if (obj1 != null)
        {
            obj1.Game = obj;
        }
        else
        {
            obj1 = new()
            {
                Game = obj,
                Mod = new(),
                Resourcepack = new(),
                Config = new()
            };
            obj1.Save();
        }

        return obj1;
    }

    public static ServerPackObj? GetOldServerPack(this GameSettingObj obj)
    {
        var file = obj.GetServerPackOldFile();
        ServerPackObj? obj1 = null;
        if (File.Exists(file))
        {
            var data = File.ReadAllText(file);
            obj1 = JsonConvert.DeserializeObject<ServerPackObj>(data);
        }

        if (obj1 != null)
        {
            obj1.Game = obj;
        }
        else
        {
            return null;
        }

        return obj1;
    }

    public static void Save(this ServerPackObj obj1)
    {
        ConfigSave.AddItem(new()
        {
            Name = $"game-server-{obj1.Game.Name}",
            Local = obj1.Game.GetServerPackFile(),
            Obj = obj1
        });
    }
}
