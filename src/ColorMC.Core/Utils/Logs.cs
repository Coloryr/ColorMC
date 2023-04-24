using ColorMC.Core.Helpers;
using System.Collections.Concurrent;

namespace ColorMC.Core.Utils;

public static class Logs
{
    private static string Local;
    private static StreamWriter Writer;
    private static Thread ThreadLog = new(Run)
    {
        Name = "ColorMC-Log"
    };
    private static ConcurrentBag<string> bags = new();
    private static bool IsRun = false;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        ColorMCCore.Stop += Stop;

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
            ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Log.Error1"), e, true);
        }
    }

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

    private static void AddText(string text)
    {
        if (!IsRun)
            return;

        bags.Add(text);
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
        AddText(text);
    }

    public static void Error(string data)
    {
        string text = $"[{DateTime.Now}][Error]{data}";
        AddText(text);
    }

    public static void Error(string data, Exception? e)
    {
        string text = $"[{DateTime.Now}][Error]{data}{Environment.NewLine}{e}";
        AddText(text);
    }

    public static void Wait()
    {
        while (!bags.IsEmpty)
            Task.Delay(100);
    }
}
