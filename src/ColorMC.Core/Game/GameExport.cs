using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace ColorMC.Core.Game;

public static class GameExport
{
    /// <summary>
    /// 到处游戏实例
    /// </summary>
    /// <param name="arg">导出参数</param>
    /// <returns></returns>
    public static async Task<bool> Export(GameExportArg arg)
    {
        using var stream = PathHelper.OpenWrite(arg.File, true);
        switch (arg.Type)
        {
            case PackType.ColorMC:
                {
                    //ColorMC整合包只需要整个文件夹打包
                    var list = arg.UnSelectItems;
                    list.Add(arg.Obj.GetModInfoJsonFile());

                    var obj1 = new Dictionary<string, ModInfoObj>();
                    foreach (var item in arg.Mods)
                    {
                        if (item.Export && item.Obj1 != null)
                        {
                            obj1.Add(item.Obj1.ModId, item.Obj1);
                        }
                    }

                    using var zip = await new ZipUtils().ZipFileAsync(arg.Obj.GetBasePath(), stream, list);
                    var data = JsonUtils.ToString(obj1, JsonType.DictionaryStringModInfoObj);
                    using var stream1 = zip.WriteToStream(Names.NameModInfoFile, new ZipWriterEntryOptions());
                    await stream1.WriteAsync(Encoding.UTF8.GetBytes(data));

                    break;
                }
            case PackType.CurseForge:
                {
                    //CF整合包需要创建信息文件
                    var obj = new CurseForgePackObj()
                    {
                        Name = arg.Name,
                        Author = arg.Author,
                        Version = arg.Version,
                        ManifestType = "minecraftModpack",
                        ManifestVersion = 1,
                        Overrides = Names.NameOverrideDir,
                        Minecraft = new CurseForgePackObj.MinecraftObj
                        {
                            Version = arg.Obj.Version,
                            ModLoaders = []
                        },
                        Files = []
                    };

                    if (arg.Obj.Loader != Loaders.Normal)
                    {
                        obj.Minecraft.ModLoaders.Add(new CurseForgePackObj.MinecraftObj.ModLoadersObj
                        {
                            Id = $"{arg.Obj.Loader.ToString().ToLower()}-{arg.Obj.LoaderVersion}",
                            Primary = true
                        });
                    }

                    //添加可以在线下载的Mod
                    foreach (var item in arg.Mods)
                    {
                        if (item.Export)
                        {
                            obj.Files.Add(new()
                            {
                                FileID = int.Parse(item.FID!),
                                ProjectID = int.Parse(item.PID!),
                                Required = true
                            });
                        }
                    }

                    //添加Mod网页链接信息
                    var data = await CurseForgeAPI.GetModsInfo(obj.Files);
                    StringBuilder html = new();
                    html.AppendLine("<ul>");
                    if (data != null)
                    {
                        foreach (var item in data.Data)
                        {
                            html.AppendLine($"<li><a href=\"{item.Links.WebsiteUrl}\">{item.Name} (by {item.Authors.GetString()})</a></li>");
                        }
                    }
                    html.AppendLine("</ul>");

                    using var s = new ZipWriter(stream, new ZipWriterOptions(CompressionType.Deflate));

                    //manifest.json
                    {
                        using var stream1 = s.WriteToStream(Names.NameManifestFile, new ZipWriterEntryOptions());
                        await stream1.WriteAsync(Encoding.UTF8.GetBytes(JsonUtils.ToString(obj, JsonType.CurseForgePackObj)));
                    }

                    //modlist.html
                    {
                        using var stream1 = s.WriteToStream(Names.NameModListFile, new ZipWriterEntryOptions());
                        await stream1.WriteAsync(Encoding.UTF8.GetBytes(html.ToString()));
                    }
                    //打包剩余文件
                    {
                        var path = arg.Obj.GetGamePath();

                        foreach (var item in arg.SelectItems)
                        {
                            var name1 = item[path.Length..];
                            name1 = name1.Replace("\\", "/");
                            if (!name1.StartsWith('/'))
                            {
                                name1 = '/' + name1;
                            }
                            name1 = Names.NameOverrideDir + name1;
                            using var buffer = PathHelper.OpenRead(item);
                            if (buffer == null)
                            {
                                continue;
                            }
                            using var stream1 = s.WriteToStream(name1, new ZipWriterEntryOptions());
                            await buffer.CopyToAsync(stream1);
                        }
                    }
                    break;
                }
            case PackType.Modrinth:
                {
                    //mo整合包信息
                    var obj = new ModrinthPackObj()
                    {
                        FormatVersion = 1,
                        Name = arg.Name,
                        VersionId = arg.Version,
                        Summary = arg.Summary,
                        Files = [],
                        Dependencies = []
                    };

                    obj.Dependencies.Add("minecraft", arg.Obj.Version);
                    switch (arg.Obj.Loader)
                    {
                        case Loaders.Forge:
                            obj.Dependencies.Add(Names.NameForgeKey, arg.Obj.LoaderVersion!);
                            break;
                        case Loaders.Fabric:
                            obj.Dependencies.Add(Names.NameFabricLoaderKey, arg.Obj.LoaderVersion!);
                            break;
                        case Loaders.Quilt:
                            obj.Dependencies.Add(Names.NameQuiltLoaderKey, arg.Obj.LoaderVersion!);
                            break;
                        case Loaders.NeoForge:
                            obj.Dependencies.Add(Names.NameNeoForgeKey, arg.Obj.LoaderVersion!);
                            break;
                    }

                    //在线模组下载
                    foreach (var item in arg.Mods)
                    {
                        if (item.Source is not { } source)
                        {
                            continue;
                        }

                        var fileObj = new ModrinthPackObj.ModrinthPackFileObj()
                        {
                            Path = $"{item.Obj1!.Path}/{item.Obj1!.File}",
                            Hashes = new ModrinthPackObj.ModrinthPackFileObj.HashObj(),
                            Downloads = [item.Url],
                            FileSize = item.FileSize,
                            ColorMc = new ColorMCSaveObj
                            {
                                Type = source,
                                FID = item.FID!,
                                PID = item.PID!
                            }
                        };
                        fileObj.Hashes.Sha1 = item.Sha1;
                        fileObj.Hashes.Sha512 = item.Sha512;
                        obj.Files.Add(fileObj);
                    }

                    //在线文件下载
                    foreach (var item in arg.OtherFiles)
                    {
                        obj.Files.Add(new ModrinthPackObj.ModrinthPackFileObj
                        {
                            Path = item.Path,
                            Hashes = new ModrinthPackObj.ModrinthPackFileObj.HashObj
                            {
                                Sha1 = item.Sha1,
                                Sha512 = item.Sha512
                            },
                            Downloads = [item.Url],
                            FileSize = item.FileSize
                        });
                    }

                    using var s = new ZipWriter(stream, new ZipWriterOptions(CompressionType.Deflate));

                    //manifest.json
                    {
                        using var stream1 = s.WriteToStream(Names.NameModrinthFile, new ZipWriterEntryOptions());
                        await stream1.WriteAsync(Encoding.UTF8.GetBytes(JsonUtils.ToString(obj, JsonType.ModrinthPackObj)));
                    }
                    //剩余文件
                    {
                        var path = arg.Obj.GetGamePath();

                        foreach (var item in arg.SelectItems)
                        {
                            var name1 = item[path.Length..];
                            name1 = name1.Replace("\\", "/");
                            if (!name1.StartsWith('/'))
                            {
                                name1 = '/' + name1;
                            }
                            name1 = Names.NameOverrideDir + name1;
                            using var buffer = PathHelper.OpenRead(item);
                            if (buffer == null)
                            {
                                continue;
                            }
                            using var stream1 = s.WriteToStream(name1, new ZipWriterEntryOptions());
                            await buffer.CopyToAsync(stream1);
                        }
                    }
                    break;
                }
        }

        return true;
    }
}
