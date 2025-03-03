using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

    /// <summary>
    /// 存档编辑器下载项目
    /// </summary>
    /// <returns></returns>
    public static DownloadItemObj BuildMcaselectorItem()
    {
        return new()
        {
            Name = "mcaselector-2.4.2",
            Local = Path.Combine(s_baseDir, "mcaselector-2.4.2.jar"),
            Url = "https://github.com/Querz/mcaselector/releases/download/2.4.2/mcaselector-2.4.2.jar"
        };
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        s_baseDir = Path.Combine(ColorMCGui.RunDir, GuiNames.NameTooslDir);

        Directory.CreateDirectory(s_baseDir);
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
