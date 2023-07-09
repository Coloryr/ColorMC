using ColorMC.Core.Helpers;
using System.Collections.Concurrent;

namespace ColorMC.Core.Utils;

/// <summary>
/// 日志
/// </summary>
public static class Logs
{
    private static readonly ConcurrentBag<string> bags = new();

    private static string Local;
    private static StreamWriter Writer;
    private static readonly Thread ThreadLog = new(Run)
    {
        Name = "ColorMC-Log"
    };
    private static bool IsRun = false;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        ColorMCCore.Stop += Stop;

        Local = dir;
        try
        {
            Writer = File.AppendText(Local + "logs.log");
            Writer.AutoFlush = true;
            IsRun = true;
            ThreadLog.Start();
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke(LanguageHelper.Get("Core.Log.Error1"), e, true);
        }
    }

    /// <summary>
    /// 运行
    /// </summary>
    private static void Run()
    {
        while (IsRun)
        {
            while (bags.TryTake(out var item))
            {
                Writer.WriteLine(item);
            }

            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    private static void Stop()
    {
        IsRun = false;
        ThreadLog.Join();

        while (bags.TryTake(out var item))
        {
            Writer.WriteLine(item);
        }
        Writer.Dispose();
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="text"></param>
    private static void AddText(string text)
    {
        if (!IsRun)
            return;

        bags.Add(text);
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

    public static string SaveCrash(string data, Exception e)
    {
        var date = DateTime.Now;
        string text = $"[{date}][Error]{data}{Environment.NewLine}{e}";

        var file = $"{Local}{date.Year}_{date.Month}_{date.Day}_" +
            $"{date.Hour}_{date.Minute}_{date.Second}_crash.log";

        File.WriteAllText(file, text);

        return file;
    }
}
