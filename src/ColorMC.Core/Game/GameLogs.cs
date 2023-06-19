using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.IO.Compression;
using System.Text;

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
        var path = Path.GetFullPath(obj.GetLogPath()) + "/";
        if (Directory.Exists(path))
        {
            var list1 = Directory.GetFiles(path);
            foreach (var item in list1)
            {
                list.Add(item.Replace(path, ""));
            }
        }

        return list;
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
            var path = Path.GetFullPath(obj.GetLogPath() + "/" + file);
            if (!File.Exists(path))
            {
                return null;
            }

            if (path.EndsWith(".log"))
            {
                return File.ReadAllText(path);
            }
            else if (path.EndsWith(".log.gz"))
            {
                using FileStream compressedFileStream = File.Open(path, FileMode.Open);
                using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                using var output = new MemoryStream();
                decompressor.CopyTo(output);
                if (SystemInfo.Os == OsType.Windows)
                {
                    return Encoding.GetEncoding("GBK").GetString(output.ToArray());
                }
                return Encoding.UTF8.GetString(output.ToArray());
            }
        }
        catch (Exception e)
        {
            Logs.Error("log error", e);
        }

        return null;
    }
}
