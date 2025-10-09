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
        if (Directory.Exists(path))
        {
            list.AddRange(Directory.GetFiles(path).Select(item => item.Replace(path, Names.NameGameCrashLogDir)));
        }

        return list;
    }

    /// <summary>
    /// 获取上一个崩溃日志
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="sec">日志与当前时间相差多少秒</param>
    /// <returns>日志位置</returns>
    public static string? GetLastCrash(this GameSettingObj obj, int sec = 5)
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
            if (time.TotalSeconds > 0 && time.TotalSeconds < sec)
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
    public static GameRuntimeLog? ReadLog(this GameSettingObj obj, string file)
    {
        try
        {
            var path = Path.GetFullPath(obj.GetGamePath() + "/" + file);
            if (!File.Exists(path))
            {
                return null;
            }

            StreamReader reader;
            using var stream = PathHelper.OpenRead(path);
            if (stream == null)
            {
                return null;
            }

            Encoding? encoding = null;
            if (obj.Encoding == LogEncoding.GBK)
            {
                try
                {
                    encoding = Encoding.GetEncoding("gbk");
                }
                catch
                {

                }
            }

            encoding ??= Encoding.UTF8;

            if (path.EndsWith(Names.NameLogExt) || path.EndsWith(Names.NameTxtExt))
            {
                reader = new StreamReader(stream, encoding);
            }
            else if (path.EndsWith(Names.NameLogGzExt))
            {
                //解压日志
                var decompressor = new GZipStream(stream, CompressionMode.Decompress);
                reader = new StreamReader(decompressor, encoding);
            }
            else
            {
                return null;
            }

            var log = new GameRuntimeLog
            {
                File = file
            };
            for (; ; )
            {
                var temp = reader.ReadLine();
                if (temp == null || reader.EndOfStream)
                {
                    break;
                }

                log.AddLog(temp);
            }

            return log;
        }
        catch (Exception e)
        {
            Logs.Error(string.Format(LanguageHelper.Get("Core.Game.Error14"), file), e);
        }

        return null;
    }
}
