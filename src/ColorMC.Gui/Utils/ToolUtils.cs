using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Net.Apis;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 工具路径
/// </summary>
public static class ToolUtils
{
    /// <summary>
    /// 基础路径
    /// </summary>
    private static string s_baseDir;

    public static string Hdiff { get; private set; }

    /// <summary>
    /// 存档编辑器下载项目
    /// </summary>
    /// <returns></returns>
    public static FileItemObj BuildMcaselectorItem()
    {
        return new()
        {
            Name = "mcaselector-2.5.2",
            Local = Path.Combine(s_baseDir, "mcaselector-2.5.2.jar"),
            Url = "https://github.com/Querz/mcaselector/releases/download/2.5.2/mcaselector-2.5.2.jar"
        };
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        s_baseDir = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameTooslDir);

        Directory.CreateDirectory(s_baseDir);
    }

    /// <summary>
    /// 获取Frp文件名
    /// </summary>
    /// <returns></returns>
    public static string GetHdiffName()
    {
        return SystemInfo.Os == OsType.Windows ? GuiNames.NameHdiffFile1 : GuiNames.NameHdiffFile;
    }

    /// <summary>
    /// 解压Frp
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="local"></param>
    /// <param name="file"></param>
    public static void Unzip(Stream stream, string local, string file)
    {
        if (file.EndsWith(Names.NameTarGzExt))
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            var tarArchive = new TarReader(gzipStream);
            TarEntry? item;
            while ((item = tarArchive.GetNextEntry()) != null)
            {
                if (item.Name != GuiNames.NameFrpFile
                    || item.Name != GuiNames.NameHdiffFile)
                {
                    continue;
                }

                item.ExtractToFileAsync(Path.Combine(local, item.Name), true);

                break;
            }

        }
        else
        {
            using var s = new ZipArchive(stream);
            foreach (var item in s.Entries)
            {
                if (item.Name != GuiNames.NameFrpFile1
                    || item.Name != GuiNames.NameFrpFile
                    || item.Name != GuiNames.NameHdiffFile
                    || item.Name != GuiNames.NameHdiffFile1)
                {
                    continue;
                }

                item.ExtractToFile(Path.Combine(local, item.Name), true);
                break;
            }
        }
    }

    /// <summary>
    /// 获取Hdiff下载文件路径
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static string GetHdiffLocal(string ver, string? filename = null)
    {
        return filename == null ?
            Path.Combine(s_baseDir, GuiNames.NameHdiffDir, ver)
            : Path.Combine(s_baseDir, GuiNames.NameHdiffDir, ver, filename);
    }

    /// <summary>
    /// 准备Hdiff文件
    /// </summary>
    /// <returns></returns>
    public static async Task<(string?, FileItemObj?)> InitHdiff()
    {
        var obj1 = await ColorMCCloudAPI.GetHdiffList();
        if (obj1 == null)
        {
            return (null, null);
        }
        var item = obj1.FirstOrDefault();
        var obj = ColorMCCloudAPI.BuildHdiffItem(item.Key, item.Value);

        if (obj.Item1 == null)
        {
            return (null, null);
        }
        else
        {
            if (File.Exists(obj.Item1))
            {
                return (obj.Item1, null);
            }

            return (obj.Item1, obj.Item2);
        }
    }

    /// <summary>
    /// 打开存档编辑器
    /// </summary>
    /// <returns></returns>
    public static async Task<MessageRes> OpenMapEditAsync()
    {
        var item = BuildMcaselectorItem();
        if (!File.Exists(item.Local))
        {
            var res = await DownloadManager.StartAsync([item]);
            if (!res)
            {
                return new MessageRes { Message = LanguageHelper.Get("Core.Tool.Error1") };
            }
        }

        var java = JvmPath.FindJava(17);
        if (java == null)
        {
            return new MessageRes { Message = LanguageHelper.Get("Core.Tool.Error2") };
        }

        var info = new ProcessStartInfo(java.Path)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        info.ArgumentList.Add("-jar");
        info.ArgumentList.Add(Path.GetFullPath(item.Local));

        var p = new Process()
        {
            EnableRaisingEvents = true,
            StartInfo = info
        };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        return new MessageRes { State = true };
    }
}
