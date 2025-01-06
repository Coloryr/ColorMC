using System.IO.Compression;
using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏日志类
/// </summary>
public static class GameLogs
{
    /// <summary>
    /// 获取游戏日志文件列表
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>列表</returns>
    public static List<string> GetLogFiles(this GameSettingObj obj)
    {
        var list = new List<string>();
        var path = Path.GetFullPath(obj.GetLogPath() + "/");
        if (Directory.Exists(path))
        {
            var list1 = Directory.GetFiles(path);
            foreach (var item in list1)
            {
                list.Add(item.Replace(path, "logs/"));
            }
        }

        path = Path.GetFullPath(obj.GetGameCrashReports() + "/");
        if (Directory.Exists(path))
        {
            var list1 = Directory.GetFiles(path);
            foreach (var item in list1)
            {
                list.Add(item.Replace(path, "crash-reports/"));
            }
        }

        return list;
    }

    public static string? GetLastCrash(this GameSettingObj obj)
    {
        var path = Path.GetFullPath(obj.GetGameCrashReports() + "/");
        if (!Directory.Exists(path))
        {
            return null;
        }

        var list = new DirectoryInfo(path).GetFiles();

        var file = list.OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
        if (file != null)
        {
            var time = file.LastWriteTime - DateTime.Now;
            if (time.TotalSeconds < 5)
            {
                return file.FullName.Replace(path, "crash-reports/");
            }
        }

        return null;
    }

    /// <summary>
    /// 读游戏日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件名</param>
    /// <returns>日志内容</returns>
    public static string? ReadLog(this GameSettingObj obj, string file)
    {
        try
        {
            var path = Path.GetFullPath(obj.GetGamePath() + "/" + file);
            if (!File.Exists(path))
            {
                return null;
            }

            if (path.EndsWith(".log") || path.EndsWith(".txt"))
            {
                return PathHelper.ReadText(path);
            }
            else if (path.EndsWith(".log.gz"))
            {
                //解压日志
                using var compressedFileStream = PathHelper.OpenRead(path)!;
                using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                using var output = new MemoryStream();
                decompressor.CopyTo(output);
                if (obj.Encoding == 1)
                {
                    try
                    {
                        return Encoding.GetEncoding("gbk").GetString(output.ToArray());
                    }
                    catch
                    { 
                        
                    }
                }
                return Encoding.UTF8.GetString(output.ToArray());
            }
        }
        catch (Exception e)
        {
            Logs.Error(string.Format(LanguageHelper.Get("Core.Game.Error14"), file), e);
        }

        return null;
    }
}
