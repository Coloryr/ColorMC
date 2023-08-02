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
/// 服务器包相关操作
/// </summary>
public static class ServerPack
{
    /// <summary>
    /// 移动到旧的服务器包
    /// </summary>
    /// <param name="obj">服务器包</param>
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

    /// <summary>
    /// 开始更新
    /// </summary>
    /// <param name="obj">服务器包</param>
    /// <returns>结果</returns>
    public static async Task<bool> Update(this ServerPackObj obj)
    {
        File.Delete(obj.Game.GetServerPackFile());
        var old = obj.Game.GetOldServerPack();

        var list5 = new List<DownloadItemObj>();

        var path = obj.Game.GetModsPath();

        old ??= new()
        {
            Mod = new()
        };

        //区分新旧mod
        ServerModItemObj?[] list1 = obj.Mod.ToArray();
        ServerModItemObj?[] list2 = old.Mod.ToArray();

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
                if (item2.Sha1 == item1?.Sha1 || item2.File == item1?.File)
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
                    Url = UrlHelper.MakeDownloadUrl(item.Source, item.Projcet!, item.FileId!, item.File)
                });
            }
        }

        var mods = await obj.Game.GetMods();

        //删除旧mod
        foreach (var item in list4)
        {
            if (item == null)
            {
                continue;
            }

            mods.Find(a => a.Sha1 == item.Sha1)?.Delete();
        }

        //检查资源包
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

        //检查配置文件
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

        //更新UI文件
        if (!string.IsNullOrWhiteSpace(obj.UI))
        {
            list5.Add(new()
            {
                Name = obj.UI,
                Local = Path.GetFullPath(ColorMCCore.BaseDir + obj.UI),
                Url = obj.Url + obj.UI
            });
        }

        //开始下载
        if (!await DownloadManager.Start(list5))
        {
            return false;
        }

        obj.Game.Save();
        obj.Save();

        return true;
    }

    /// <summary>
    /// 获取服务器包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>服务器包</returns>
    public static ServerPackObj? GetServerPack(this GameSettingObj obj)
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

        return obj1;
    }

    /// <summary>
    /// 获取旧的服务器包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>服务器包</returns>
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

    /// <summary>
    /// 保存服务器包
    /// </summary>
    /// <param name="obj">服务器包</param>
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
    /// 生成服务器包
    /// </summary>
    /// <param name="obj">服务器包</param>
    /// <param name="local">保存路径</param>
    /// <returns>创建结果</returns>
    public static async Task<bool> GenServerPack(this ServerPackObj obj, string local)
    {
        var obj1 = new ServerPackObj()
        {
            MCVersion = obj.Game.Version,
            Loader = obj.Game.Loader,
            LoaderVersion = obj.Game.LoaderVersion,
            Url = obj.Url,
            Version = obj.Version,
            Text = obj.Text,
            ForceUpdate = obj.ForceUpdate,
            Mod = new(),
            Resourcepack = new(),
            Config = new()
        };

        if (!local.EndsWith("/"))
        {
            local += "/";
        }

        bool fail = false;

        var task1 = Task.Run(() =>
        {
            try
            {
                var local1 = local + "mods/";
                Directory.CreateDirectory(local1);
                var local2 = obj.Game.GetModsPath();

                foreach (var item in obj.Mod)
                {
                    if (item.Source == null)
                    {
                        var name = Path.GetFullPath(local1 + item.File);
                        var name1 = Path.GetFullPath(local2 + item.File);
                        if (File.Exists(name))
                        {
                            File.Delete(name);
                        }
                        File.Copy(name1, name);
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
            try
            {
                var local1 = local + "resourcepacks/";
                Directory.CreateDirectory(local1);
                var local2 = obj.Game.GetResourcepacksPath();

                foreach (var item in obj.Resourcepack)
                {
                    var name = Path.GetFullPath(local1 + item.File);
                    var name1 = Path.GetFullPath(local2 + item.File);
                    if (File.Exists(name))
                    {
                        File.Delete(name);
                    }
                    File.Copy(name1, name);

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
            try
            {
                var local1 = local + "config/";
                Directory.CreateDirectory(local1);
                var local2 = obj.Game.GetGamePath();

                foreach (var item in obj.Config)
                {
                    string path1 = Path.GetFullPath(local2 + "/" + item.Group);
                    string path2 = Path.GetFullPath(local1 + item.Group);
                    if (item.Dir)
                    {
                        if (item.Zip)
                        {
                            var file = new FileInfo(path2[..^1] + ".zip");
                            await ZipUtils.ZipFile(path1, file.FullName);
                            using var stream = File.OpenRead(file.FullName);

                            var item1 = new ConfigPackObj()
                            {
                                FileName = file.Name,
                                Group = item.Group,
                                Sha1 = Funtions.GenSha1(stream),
                                Zip = item.Zip,
                                Url = item.Url,
                                Dir = true
                            };

                            obj1.Config.Add(item1);
                        }
                        else
                        {
                            var files = PathC.GetAllFile(path1);
                            foreach (var item1 in files)
                            {
                                var name = item1.FullName.Replace(path1, "").Replace("\\", "/");
                                var file1 = new FileInfo(path2 + name);
                                file1.Directory?.Create();
                                if (File.Exists(file1.FullName))
                                {
                                    File.Delete(file1.FullName);
                                }
                                File.Copy(item1.FullName, file1.FullName);
                                using var stream = File.OpenRead(file1.FullName);

                                var item2 = new ConfigPackObj()
                                {
                                    Group = item.Group,
                                    FileName = name,
                                    Sha1 = Funtions.GenSha1(stream),
                                    Url = item.Url + name,
                                    Zip = false,
                                    Dir = false
                                };

                                obj1.Config.Add(item2);
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(path2))
                        {
                            File.Delete(path2);
                        }
                        File.Copy(path1, path2);
                        using var stream = File.OpenRead(path2);

                        var item2 = new ConfigPackObj()
                        {
                            Group = "",
                            FileName = item.Group,
                            Sha1 = Funtions.GenSha1(stream),
                            Url = item.Url,
                            Zip = item.Zip,
                            Dir = false
                        };

                        obj1.Config.Add(item2);
                    }
                }
            }
            catch (Exception e)
            {
                fail = true;
                Logs.Error(LanguageHelper.Get("Core.ServerPack.Error1"), e);
            }
        });

        await Task.WhenAll(task1, task2, task3);

        if (!string.IsNullOrWhiteSpace(obj.UI) && File.Exists(obj.UI))
        {
            obj1.UI = Path.GetFileName(obj.UI);
            var file = local + obj1.UI;
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.Copy(obj.UI, file);
        }

        File.WriteAllText(Path.GetFullPath(local + "server.json"),
            JsonConvert.SerializeObject(obj1));

        return !fail;
    }

    /// <summary>
    /// 检查服务器包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="url">网址</param>
    /// <returns>结果</returns>
    public static async Task<(bool Res, ServerPackObj? Obj)> ServerPackCheck(this GameSettingObj obj, string url)
    {
        var data = await BaseClient.GetString(url + "server.json");
        if (data.Item1 == false)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Http.Error7"),
                    new Exception(url), false);
            return (false, null);
        }
        var obj1 = JsonConvert.DeserializeObject<ServerPackObj>(data.Item2!);
        if (obj1 == null)
        {
            return (false, null);
        }

        var obj2 = obj.GetServerPack();
        if (obj2 == null || obj1.Version != obj2.Version)
        {
            if (ColorMCCore.UpdateSelect == null)
            {
                return (false, null);
            }
            if (!obj1.ForceUpdate && await ColorMCCore.UpdateSelect(obj1.Text))
            {
                return (false, null);
            }

            obj2?.MoveToOld();
            obj1.Game = obj;

            ColorMCCore.UpdateState?.Invoke(LanguageHelper.Get("Core.ServerPack.Info1"));

            var res = await obj1.Update();
            return (res, obj1);
        }

        return (false, null);
    }
}
