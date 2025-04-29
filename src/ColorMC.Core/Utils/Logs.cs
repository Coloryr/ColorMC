using System.Collections.Concurrent;
using ColorMC.Core.Helpers;

namespace ColorMC.Core.Utils;

/// <summary>
/// 日志
/// </summary>
public static class Logs
{
    /// <summary>
    /// 需要写入的日志列表
    /// </summary>
    private static readonly ConcurrentBag<string> s_bags = [];

    /// <summary>
    /// 日志文件位置
    /// </summary>
    private static string s_local;
    /// <summary>
    /// 日志文件流
    /// </summary>
    private static StreamWriter s_writer;
    /// <summary>
    /// 日志写入线程
    /// </summary>
    private static readonly Thread t_log = new(Run)
    {
        Name = "ColorMC Log"
    };
    /// <summary>
    /// 是否在运行中
    /// </summary>
    private static bool s_run = false;

    /// <summary>
    /// 初始化
    /// </summary>
    internal static void Init()
    {
        ColorMCCore.Stop += Stop;

        s_local = ColorMCCore.BaseDir;
        try
        {
            var stream = File.Open(Path.Combine(s_local, Names.NameLogFile), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.Seek(0, SeekOrigin.End);
            s_writer = new(stream)
            {
                AutoFlush = true
            };
            s_run = true;
            t_log.Start();
        }
        catch
        {

        }
    }

    /// <summary>
    /// 运行
    /// </summary>
    private static void Run()
    {
        while (s_run)
        {
            lock (s_bags)
            {
                while (s_bags.TryTake(out var item))
                {
                    s_writer.WriteLine(item);
                }
            }

            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    private static void Stop()
    {
        s_run = false;
        lock (s_bags)
        {
            while (s_bags.TryTake(out var item))
            {
                s_writer.WriteLine(item);
            }
            s_writer.Dispose();
        }
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="text"></param>
    private static void AddText(string text)
    {
        if (!s_run)
            return;

        s_bags.Add(text);
    }

    /// <summary>
    /// 信息
    /// </summary>
    /// <param name="data"></param>
    public static void Info(string data)
    {
        string text = $"[{DateTime.Now}][Info]{data}";
        Console.WriteLine(text);
        AddText(text);
    }

    /// <summary>
    /// 警告
    /// </summary>
    /// <param name="data"></param>
    public static void Warn(string data)
    {
        string text = $"[{DateTime.Now}][Warn]{data}";
        AddText(text);
    }

    /// <summary>
    /// 错误
    /// </summary>
    /// <param name="data"></param>
    public static void Error(string data)
    {
        string text = $"[{DateTime.Now}][Error]{data}";
        AddText(text);
    }

    /// <summary>
    /// 错误
    /// </summary>
    /// <param name="data"></param>
    /// <param name="e"></param>
    public static void Error(string data, Exception? e)
    {
        string text = $"[{DateTime.Now}][Error]{data}{Environment.NewLine}{e}";
        AddText(text);
    }

    /// <summary>
    /// 保存崩溃日志
    /// </summary>
    /// <param name="data">消息</param>
    /// <param name="e">错误内容</param>
    /// <returns></returns>
    public static string Crash(string data, Exception e)
    {
        var date = DateTime.Now;
        string text = $"ColorMC Version: {ColorMCCore.Version}{Environment.NewLine}" +
            $"System: {SystemInfo.System}{Environment.NewLine}" +
            $"SystemName: {SystemInfo.SystemName}{Environment.NewLine}" +
            $"{data}{Environment.NewLine}" +
            $"{e}";

        var file = $"{date.Year}_{date.Month}_{date.Day}_" +
            $"{date.Hour}_{date.Minute}_{date.Second}_crash.log";
        file = Path.Combine(s_local, file);
        PathHelper.WriteText(file, text);
        Console.WriteLine(text);

        return file;
    }
}
