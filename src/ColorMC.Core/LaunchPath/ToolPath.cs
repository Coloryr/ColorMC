using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.LaunchPath;

public class ToolPath
{
    public const string Name = "tools";

    /// <summary>
    /// 基础路径
    /// </summary>
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行目录</param>
    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);
    }

    public static DownloadItemObj GetMcaselectorDownloadItem()
    {
        return new()
        {
             Name = "mcaselector-2.2.2", 
             Local = $"{BaseDir}/mcaselector-2.2.2.jar",
             Url = "https://github.com/Querz/mcaselector/releases/download/2.2.2/mcaselector-2.2.2.jar"
        };
    }

    public static async Task<(bool, string?)> OpenMapEdit()
    {
        var item = GetMcaselectorDownloadItem();
        if (!File.Exists(item.Local))
        {
            var res = await DownloadManager.Start(new() { item });
            if (!res)
            {
                return (false, LanguageHelper.Get("Core.Tool.Error1"));
            }
        }

        var java = JvmPath.FindJava(17);
        if (java == null)
        {
            return (false, LanguageHelper.Get("Core.Tool.Error2"));
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

        return (true, null);
    }
}
