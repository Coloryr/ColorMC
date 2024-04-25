using System.Collections.Concurrent;
using ColorMC.Core.Helpers;

namespace ColorMC.Core.Utils;

/// <summary>
/// 日志
/// </summary>
public static class Logs
{
    private static readonly ConcurrentBag<string> s_bags = [];

    private static string s_local;
    private static StreamWriter s_writer;
    private static readonly Thread t_log = new(Run)
    {
        Name = "ColorMC-Log"
    };
    private static bool s_run = false;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        ColorMCCore.Stop += Stop;

        s_local = dir;
        try
        {
            s_writer = File.AppendText(s_local + "logs.log");
            s_writer.AutoFlush = true;
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
        string text = $"Version:{ColorMCCore.Version}{Environment.NewLine}" +
            $"System:{SystemInfo.System}{Environment.NewLine}" +
            $"SystemName:{SystemInfo.SystemName}" +
            $"{data}" +
            $"{Environment.NewLine}{e}";

        var file = $"{s_local}{date.Year}_{date.Month}_{date.Day}_" +
            $"{date.Hour}_{date.Minute}_{date.Second}_crash.log";

        PathHelper.WriteText(file, text);

        return file;
    }
}
