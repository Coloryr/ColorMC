using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Game;

/// <summary>
/// 服务器实例相关操作
/// </summary>
public static class ServerPack
{
    /// <summary>
    /// 移动到旧的服务器实例
    /// </summary>
    /// <param name="obj">服务器实例</param>
    public static void MoveToOld(this ServerPackObj obj)
    {
        var file1 = obj.Game.GetServerPackOldFile();
        var file2 = obj.Game.GetServerPackFile();

        PathHelper.MoveFile(file2, file1);
    }

    /// <summary>
    /// 开始更新
    /// </summary>
    /// <param name="obj">服务器实例</param>
    /// <returns>结果</returns>
    public static async Task<bool> UpdateAsync(this ServerPackObj obj, GameSettingObj game,
        ColorMCCore.UpdateState state)
    {
        PathHelper.Delete(game.GetServerPackFile());
        var old = game.GetOldServerPack();

        //替换旧的实例
        obj.Game.CopyObj(game);

        var list5 = new List<DownloadItemObj>();

        var path = game.GetModsPath();

        old ??= new()
        {
            Mod = []
        };

        state(LanguageHelper.Get("Core.ServerPack.Info2"));

        //区分新旧mod
        ServerModItemObj?[] list1 = [.. obj.Mod];
        ServerModItemObj?[] list2 = [.. old.Mod];

        for (int a = 0; a < list2.Length; a++)
        {
            var item1 = list1[a];
            for (int b = 0; b < list2.Length; b++)
            {
                var item2 = list2[a];
                if (item2 == null)
                {
                    continue;
                }
                if (item2.Sha256 == item1?.Sha256 || item2.File == item1?.File)
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

        //添加新mod
        foreach (var item in list3)
        {
            if (item == null)
            {
                continue;
            }
            if (item.Source == null)
            {
                list5.Add(new()
                {
                    Name = item.File,
                    Local = Path.GetFullPath(path + item.File),
                    SHA256 = item.Sha256,
                    Url = game.ServerUrl + item.Url,
                    UseColorMCHead = true
                });
            }
            else
            {
                list5.Add(new()
                {
                    Name = item.File,
                    Local = Path.GetFullPath(path + item.File),
                    SHA256 = item.Sha256,
                    Url = UrlHelper.MakeDownloadUrl(item.Source, item.Projcet!, item.FileId!, item.File),
                });
            }
        }

        var mods = await game.GetModsAsync(true);

        //删除旧mod
        foreach (var item in list4)
        {
            if (item == null)
            {
                continue;
            }

            mods.Find(a => a.Sha256 == item.Sha256)?.Delete();
        }

        //检查资源包
        path = game.GetResourcepacksPath();

        foreach (var item in obj.Resourcepack)
        {
            list5.Add(new()
            {
                Name = item.File,
                Local = Path.GetFullPath(path + item.File),
                SHA256 = item.Sha256,
                Url = game.ServerUrl + item.Url,
                UseColorMCHead = true
            });
        }

        //检查配置文件
        path = game.GetGamePath();
        var path1 = game.GetBasePath();

        foreach (var item in obj.Config)
        {
            if (item.IsZip)
            {
                list5.Add(new()
                {
                    Name = item.FileName,
                    Local = Path.GetFullPath(path1 + "/" + item.FileName),
                    SHA256 = item.Sha256,
                    Url = game.ServerUrl + item.Url,
                    Overwrite = true,
                    UseColorMCHead = true,
                    Later = (stream) =>
                    {
                        if (item.IsZip)
                        {
                            new ZipUtils().UnzipAsync(Path.GetFullPath(path + "/" + item.Group), "", stream).Wait();
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
                    SHA256 = item.Sha256,
                    Overwrite = true,
                    UseColorMCHead = true,
                    Url = game.ServerUrl + item.Url
                });
            }
        }

        state(LanguageHelper.Get("Core.ServerPack.Info3"));
        //开始下载
        var res = await DownloadManager.StartAsync(list5);
        state(null);
        return res;
    }

    /// <summary>
    /// 获取服务器实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>服务器实例</returns>
    public static (string?, ServerPackObj?) GetServerPack(this GameSettingObj obj)
    {
        var file = obj.GetServerPackFile();
        ServerPackObj? obj1 = null;
        string? sha1 = null;
        if (File.Exists(file))
        {
            using var stream = PathHelper.OpenRead(file)!;
            sha1 = HashHelper.GenSha1(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var data = PathHelper.ReadText(stream)!;
            obj1 = JsonConvert.DeserializeObject<ServerPackObj>(data);
        }

        if (obj1 != null)
        {
            obj1.Game = obj;
        }

        return (sha1, obj1);
    }

    /// <summary>
    /// 获取旧的服务器实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>服务器实例</returns>
    public static ServerPackObj? GetOldServerPack(this GameSettingObj obj)
    {
        var file = obj.GetServerPackOldFile();
        ServerPackObj? obj1 = null;
        if (File.Exists(file))
        {
            var data = PathHelper.ReadText(file)!;
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

    /// <summary>
    /// 保存服务器实例
    /// </summary>
    /// <param name="obj">服务器实例</param>
    public static void Save(this ServerPackObj obj)
    {
        ConfigSave.AddItem(new()
        {
            Name = $"game-server-{obj.Game.Name}",
            Local = obj.Game.GetServerPackFile(),
            Obj = obj
        });
    }

    /// <summary>
    /// 生成服务器实例
    /// </summary>
    /// <param name="obj">服务器实例</param>
    /// <param name="local">保存路径</param>
    /// <returns>创建结果</returns>
    public static async Task<bool> GenServerPackAsync(this ServerPackObj obj, string local,
        ColorMCCore.Request request)
    {
        var obj1 = new ServerPackObj()
        {
            Mod = [],
            Resourcepack = [],
            Config = [],
            Game = obj.Game,
            Text = obj.Text
        };

        if (!local.EndsWith('/') && !local.EndsWith('\\'))
        {
            local += "/";
        }

        var path = local;

        local += "files/";

        await PathHelper.DeleteFilesAsync(local, request);
        Directory.CreateDirectory(local);

        bool fail = false;

        var task1 = Task.Run(() =>
        {
            //Mods
            try
            {
                var local2 = obj.Game.GetModsPath();

                foreach (var item in obj.Mod)
                {
                    if (item.Source == null)
                    {
                        var name = Path.GetFullPath(local + item.Sha256);
                        var name1 = Path.GetFullPath(local2 + item.File);
                        PathHelper.CopyFile(name1, name);
                        item.Url = $"files/{item.Sha256}";
                    }

                    obj1.Mod.Add(item);
                }
            }
            catch (Exception e)
            {
                fail = true;
                Logs.Error(LanguageHelper.Get("Core.ServerPack.Error1"), e);
            }
        });

        var task2 = Task.Run(() =>
        {
            //资源包
            try
            {
                var local2 = obj.Game.GetResourcepacksPath();

                foreach (var item in obj.Resourcepack)
                {
                    var name = Path.GetFullPath(local + item.Sha256);
                    var name1 = Path.GetFullPath(local2 + item.File);
                    PathHelper.CopyFile(name1, name);
                    item.Url = $"files/{item.Sha256}";
                    obj1.Resourcepack.Add(item);
                }
            }
            catch (Exception e)
            {
                fail = true;
                Logs.Error(LanguageHelper.Get("Core.ServerPack.Error1"), e);
            }
        });

        var task3 = Task.Run(async () =>
        {
            //配置文件
            try
            {
                var local2 = obj.Game.GetGamePath();

                foreach (var item in obj.Config)
                {
                    string path1 = Path.GetFullPath(local2 + "/" + item.Group);
                    if (item.IsDir)
                    {
                        string path2 = Path.GetFullPath(local + item.Group);
                        if (item.IsZip)
                        {
                            //打包进压缩包
                            var file = new FileInfo(path2[..^1] + ".zip");
                            await new ZipUtils(GameRequest: request).ZipFileAsync(path1, file.FullName);
                            var stream = PathHelper.OpenRead(file.FullName)!;

                            var item1 = new ConfigPackObj()
                            {
                                FileName = file.Name,
                                Group = item.Group,
                                Sha256 = HashHelper.GenSha256(stream),
                                IsZip = true
                            };
                            item1.Url = $"files/{item1.Sha256}";

                            stream.Dispose();

                            PathHelper.MoveFile(file.FullName, Path.GetFullPath(local + item1.Sha256));

                            obj1.Config.Add(item1);
                        }
                        else
                        {
                            //复制文件
                            var files = PathHelper.GetAllFile(path1);
                            foreach (var item1 in files)
                            {
                                var name = item1.FullName.Replace(path1, "").Replace("\\", "/");
                                using var stream = PathHelper.OpenRead(item1.FullName)!;

                                var item2 = new ConfigPackObj()
                                {
                                    Group = item.Group,
                                    FileName = name,
                                    Sha256 = HashHelper.GenSha256(stream),
                                    IsZip = false
                                };

                                PathHelper.CopyFile(item1.FullName, local + item2.Sha256);

                                item2.Url = $"files/{item2.Sha256}";

                                obj1.Config.Add(item2);
                            }
                        }
                    }
                    else
                    {
                        //复制文件
                        using var stream = PathHelper.OpenRead(path1)!;

                        var item2 = new ConfigPackObj()
                        {
                            Group = "",
                            FileName = item.Group,
                            Sha256 = HashHelper.GenSha256(stream),
                            IsZip = false
                        };

                        item2.Url = $"files/{item2.Sha256}";

                        PathHelper.CopyFile(path1, local + item2.Sha256);

                        obj1.Config.Add(item2);
                    }
                }

                var icon = obj.Game.GetIconFile();
                if (File.Exists(icon))
                {
                    using var stream = PathHelper.OpenRead(icon)!;

                    var item2 = new ConfigPackObj()
                    {
                        Group = "../",
                        FileName = InstancesPath.Name10,
                        Sha256 = HashHelper.GenSha256(stream),
                        IsZip = false
                    };

                    item2.Url = $"files/{item2.Sha256}";

                    PathHelper.CopyFile(icon, local + item2.Sha256);

                    obj1.Config.Add(item2);
                }
            }
            catch (Exception e)
            {
                fail = true;
                Logs.Error(LanguageHelper.Get("Core.ServerPack.Error1"), e);
            }
        });

        await Task.WhenAll(task1, task2, task3);

        PathHelper.WriteText(Path.GetFullPath(path + "server.json"),
            JsonConvert.SerializeObject(obj1));

        using var stream = PathHelper.OpenRead(path + "server.json")!;

        PathHelper.WriteText(Path.GetFullPath(path + "sha1"),
            await HashHelper.GenSha1Async(stream));

        return !fail;
    }

    /// <summary>
    /// 检查服务器实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="url">网址</param>
    /// <returns>结果</returns>
    public static async Task<bool> ServerPackCheckAsync(this GameSettingObj obj,
        ColorMCCore.UpdateState state, ColorMCCore.ChoiseCall select)
    {
        var obj2 = obj.GetServerPack();
        var res = await BaseClient.GetStringAsync(obj.ServerUrl + "sha1");
        if (!res.Item1)
        {
            return false;
        }
        if (obj2.Item1 == null || obj2.Item1 != res.Item2)
        {
            var res1 = await BaseClient.GetStringAsync(obj.ServerUrl + "server.json");
            if (!res1.Item1)
            {
                return false;
            }

            ServerPackObj? obj1;
            try
            {
                obj1 = JsonConvert.DeserializeObject<ServerPackObj>(res1.Item2!);
                if (obj1 == null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Error12"), e);
                return false;
            }
            if (!await select(obj1.Text))
            {
                return true;
            }

            obj2.Item2?.MoveToOld();
            state(LanguageHelper.Get("Core.ServerPack.Info1"));

            var res2 = await obj1.UpdateAsync(obj, state);
            if (res2)
            {
                File.WriteAllText(obj.GetServerPackFile(), res1.Item2!);
            }

            return res2;
        }

        return true;
    }
}
