using System.Diagnostics;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// 工具路径
/// </summary>
public static class ToolPath
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

    /// <summary>
    /// 打开地图编辑器
    /// </summary>
    /// <returns></returns>
    public static async Task<MessageRes> OpenMapEditAsync()
    {
        var item = DownloadItemHelper.BuildMcaselectorItem();
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
