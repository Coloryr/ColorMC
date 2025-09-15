using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

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
        var path = obj.GetLogPath();
        if (Directory.Exists(path))
        {
            list.AddRange(Directory.GetFiles(path).Select(item => item.Replace(path, Names.NameGameLogDir)));
        }

        path = obj.GetGameCrashReports();
        if (!Directory.Exists(path))
        {
            return list;
        }

        list.AddRange(Directory.GetFiles(path).Select(item => item.Replace(path, Names.NameGameCrashLogDir)));

        return list;
    }

    /// <summary>
    /// 获取上一个崩溃日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>日志位置</returns>
    public static string? GetLastCrash(this GameSettingObj obj)
    {
        var path = obj.GetGameCrashReports();
        if (!Directory.Exists(path))
        {
            return null;
        }

        var list = new DirectoryInfo(path).GetFiles();

        var file = list.OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
        if (file != null)
        {
            var time = DateTime.Now - file.LastWriteTime;
            if (time.TotalSeconds is > 0 and < 5)
            {
                return file.FullName.Replace(path, Names.NameGameCrashLogDir);
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
            var path = Path.Combine(obj.GetGamePath(), file);
            if (!File.Exists(path))
            {
                return null;
            }

            if (path.EndsWith(Names.NameLogExt) || path.EndsWith(Names.NameTxtExt))
            {
                return PathHelper.ReadText(path);
            }
            else if (path.EndsWith(Names.NameLogGzExt))
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
