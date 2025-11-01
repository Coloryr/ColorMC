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

/// <summary>
/// 游戏实例导出
/// </summary>
public static class GameExport
{
    /// <summary>
    /// 往压缩包里面添加文件
    /// </summary>
    /// <param name="zip">压缩包</param>
    /// <param name="name">文件名</param>
    /// <param name="data">文件内容</param>
    /// <returns></returns>
    private static async Task WriteItemAsync(ZipWriter zip, string name, string data)
    {
        using var stream1 = zip.WriteToStream(name, new ZipWriterEntryOptions());
        await stream1.WriteAsync(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// 打包ColorMC格式整合包
    /// </summary>
    /// <param name="arg">导出参数</param>
    /// <param name="stream">导出压缩包</param>
    /// <returns></returns>
    private static async Task ColorMCAsync(GameExportArg arg, Stream stream)
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

        using var zip = await new ZipProcess().ZipFileAsync(arg.Obj.GetBasePath(), stream, list);
        await WriteItemAsync(zip, Names.NameModInfoFile, JsonUtils.ToString(obj1, JsonType.DictionaryStringModInfoObj));
    }

    /// <summary>
    /// 导出CurseForge格式整合包
    /// </summary>
    /// <param name="arg">导出参数</param>
    /// <param name="stream">导出压缩包</param>
    /// <returns></returns>
    private static async Task CurseForgeAsync(GameExportArg arg, Stream stream)
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
            Minecraft = new()
            {
                Version = arg.Obj.Version,
                ModLoaders = []
            },
            Files = []
        };

        if (arg.Obj.Loader != Loaders.Normal)
        {
            obj.Minecraft.ModLoaders.Add(new()
            {
                Id = $"{arg.Obj.Loader.ToString().ToLower()}-{arg.Obj.LoaderVersion}",
                Primary = true
            });
        }

        //添加可以在线下载的Mod
        foreach (var item in arg.Mods)
        {
            if (!item.Export)
            {
                continue;
            }

            obj.Files.Add(new()
            {
                FileID = int.Parse(item.FID!),
                ProjectID = int.Parse(item.PID!),
                Required = true
            });
        }

        //添加Mod网页链接信息
        var data = await CurseForgeAPI.GetModsInfoAsync(obj.Files);
        var html = new StringBuilder();
        html.AppendLine("<ul>");
        if (data != null)
        {
            foreach (var item in data.Data)
            {
                html.AppendLine($"<li><a href=\"{item.Links.WebsiteUrl}\">{item.Name} (by {item.Authors.GetString()})</a></li>");
            }
        }
        html.AppendLine("</ul>");

        using var zip = new ZipWriter(stream, new ZipWriterOptions(CompressionType.Deflate));

        //manifest.json
        await WriteItemAsync(zip, Names.NameManifestFile, JsonUtils.ToString(obj, JsonType.CurseForgePackObj));
        //modlist.html
        await WriteItemAsync(zip, Names.NameModListFile, html.ToString());

        //打包剩余文件
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
            using var stream1 = zip.WriteToStream(name1, new ZipWriterEntryOptions());
            await buffer.CopyToAsync(stream1);
        }
    }

    /// <summary>
    /// 导出Modrinth格式整合包
    /// </summary>
    /// <param name="arg">导出参数</param>
    /// <param name="stream">导出压缩包</param>
    /// <returns></returns>
    private static async Task ModrinthAsync(GameExportArg arg, Stream stream)
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

            obj.Files.Add(new()
            {
                Path = $"{item.Obj1!.Path}/{item.Obj1!.File}",
                Hashes = new()
                {
                    Sha1 = item.Sha1,
                    Sha512 = item.Sha512
                },
                Downloads = [item.Url],
                FileSize = item.FileSize,
                ColorMc = new ColorMCSaveObj
                {
                    Type = source,
                    FID = item.FID!,
                    PID = item.PID!
                }
            });
        }

        //在线文件下载
        foreach (var item in arg.OtherFiles)
        {
            obj.Files.Add(new()
            {
                Path = item.Path,
                Hashes = new()
                {
                    Sha1 = item.Sha1,
                    Sha512 = item.Sha512
                },
                Downloads = [item.Url],
                FileSize = item.FileSize
            });
        }

        using var zip = new ZipWriter(stream, new ZipWriterOptions(CompressionType.Deflate));

        //manifest.json
        await WriteItemAsync(zip, Names.NameModrinthFile, JsonUtils.ToString(obj, JsonType.ModrinthPackObj));

        //剩余文件
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
            using var stream2 = zip.WriteToStream(name1, new ZipWriterEntryOptions());
            await buffer.CopyToAsync(stream2);
        }
    }

    /// <summary>
    /// 导出游戏实例
    /// </summary>
    /// <param name="arg">导出参数</param>
    /// <returns></returns>
    public static async Task<bool> ExportAsync(GameExportArg arg)
    {
        using var stream = PathHelper.OpenWrite(arg.File);

        switch (arg.Type)
        {
            case PackType.ColorMC:
                await ColorMCAsync(arg, stream);
                break;
            case PackType.CurseForge:
                await CurseForgeAsync(arg, stream);
                break;
            case PackType.Modrinth:
                await ModrinthAsync(arg, stream);
                break;
            default:
                throw new ArgumentOutOfRangeException(arg.Type.ToString());
        }

        return true;
    }
}
