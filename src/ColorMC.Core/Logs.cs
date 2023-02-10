

using ColorMC.Core.Utils;
using System.Collections.Concurrent;
using static System.Net.Mime.MediaTypeNames;

namespace ColorMC.Core;

public static class Logs
{
    private static string Local;
    private static StreamWriter Writer;
    private static Thread ThreadLog = new(Run)
    {
        Name = "ColorMC-Log"
    };
    private static ConcurrentBag<string> bags = new();
    private static Semaphore semaphore = new(0, 10);
    private static bool IsRun = false;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        Local = dir + "logs.log";

        try
        {
            Writer = File.AppendText(Local);
            Writer.AutoFlush = true;
            IsRun = true;
            ThreadLog.Start();
        }
        catch (Exception e)
        {
            CoreMain.OnError?.Invoke(LanguageHelper.GetName("Log.Init.Error"), e, true);
        }
    }

    private static void Run()
    {
        while (IsRun)
        {
            semaphore.WaitOne();
            while (bags.TryTake(out var item))
            {
                Writer.WriteLine(item);
            }
        }
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public static void Stop()
    {
        IsRun = false;
        semaphore.Release();
    }

    private static void AddText(string text)
    {
        if (!IsRun)
            return;

        bags.Add(text);
        semaphore.Release();
    }

    public static void Info(string data)
    {
        string text = $"[{DateTime.Now}][Info]{data}";
        Console.WriteLine(text);
        AddText(text);
    }

    public static void Warn(string data)
    {
        string text = $"[{DateTime.Now}][Warn]{data}";
        Console.WriteLine(text);
        AddText(text);
    }

    public static void Error(string data)
    {
        string text = $"[{DateTime.Now}][Error]{data}";
        Console.WriteLine(text);
        AddText(text);
    }

    public static void Error(string data, Exception e)
    {
        string text = $"[{DateTime.Now}][Error]{data}{Environment.NewLine}{e}";
        Console.WriteLine(text);
        AddText(text);
    }
}
