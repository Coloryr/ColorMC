using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

public static class GameExport
{
    public static async Task<bool> Export(GameExportArg arg)
    {
        if (arg.Type == PackType.ColorMC)
        {
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

            await new ZipUtils().ZipFileAsync(arg.Obj.GetBasePath(),
                arg.File, list);

            using var stream = PathHelper.OpenWrite(arg.File, false);
            using var s = new ZipArchive(stream, ZipArchiveMode.Update);
            var data = JsonUtils.ToString(obj1, JsonType.DictionaryStringModInfoObj);
            var item1 = s.CreateEntry(Names.NameModInfoFile);
            using var s1 = item1.Open();
            await s1.WriteAsync(Encoding.UTF8.GetBytes(data));
            return true;
        }
        else if (arg.Type == PackType.CurseForge)
        {
            var obj = new CurseForgePackObj()
            {
                Name = arg.Name,
                Author = arg.Author,
                Version = arg.Version,
                ManifestType = "minecraftModpack",
                ManifestVersion = 1,
                Overrides = "overrides",
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

            using var stream = PathHelper.OpenWrite(arg.File, true);
            using var s = new ZipArchive(stream, ZipArchiveMode.Create);

            //manifest.json
            {
                byte[] buffer = Encoding.UTF8.GetBytes(JsonUtils.ToString(obj, JsonType.CurseForgePackObj));
                var entry = s.CreateEntry("manifest.json");
                using var stream1 = entry.Open();
                await stream1.WriteAsync(buffer);
            }

            //modlist.html
            {
                byte[] buffer = Encoding.UTF8.GetBytes(html.ToString());
                var entry = s.CreateEntry("modlist.html");
                using var stream1 = entry.Open();
                await stream1.WriteAsync(buffer);
            }

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
                    name1 = "overrides" + name1;
                    using var buffer = PathHelper.OpenRead(item);
                    if (buffer == null)
                    {
                        continue;
                    }
                    var entry = s.CreateEntry(name1);
                    using var stream1 = entry.Open();
                    await buffer.CopyToAsync(stream1);
                }
            }
        }
        else if (arg.Type == PackType.Modrinth)
        {
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

            foreach (var item in arg.Mods)
            {
                if (item.Source is { } source)
                {
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
                        ColorMc = new()
                        { 
                            Type = source,
                            FID = item.FID!,
                            PID = item.PID!
                        }
                    });
                }
            }

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

            using var stream = PathHelper.OpenWrite(arg.File, true);
            using var s = new ZipArchive(stream, ZipArchiveMode.Create);

            //manifest.json
            {
                byte[] buffer = Encoding.UTF8.GetBytes(JsonUtils.ToString(obj, JsonType.ModrinthPackObj));
                var entry = s.CreateEntry("modrinth.index.json");
                using var stream1 = entry.Open();
                await stream1.WriteAsync(buffer);
            }

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
                    name1 = "overrides" + name1;
                    using var buffer = PathHelper.OpenRead(item);
                    if (buffer == null)
                    {
                        continue;
                    }
                    var entry = s.CreateEntry(name1);
                    using var stream1 = entry.Open();
                    await buffer.CopyToAsync(stream1);
                }
            }
        }
        return true;
    }
}
