

using System.Collections.Concurrent;

namespace ColorMC.Core;

public static class Logs
{
    private static string Local;
    private static StreamWriter Writer;
    private static Thread thread = new(Run)
    {
        Name = "ColorMC-Log"
    };
    private static ConcurrentBag<string> bags = new();
    private static Semaphore semaphore = new(0, 10);

    public static void Init(string dir)
    {
        Local = dir + "logs.log";

        try
        {
            Writer = File.AppendText(Local);
            Writer.AutoFlush = true;
            thread.Start();
        }
        catch (Exception e)
        {
            CoreMain.OnError?.Invoke("日志文件初始化错误", e, true);
        }
    }

    private static void Run()
    {
        while (true)
        {
            semaphore.WaitOne();
            while (bags.TryTake(out var item))
            {
                Writer.WriteLine(item);
            }
        }
    }

    public static void Info(string data)
    {
        string text = $"[{DateTime.Now}][Info]{data}";
        bags.Add(text);
        semaphore.Release();
        Console.WriteLine(text);
    }

    public static void Warn(string data)
    {
        string text = $"[{DateTime.Now}][Warn]{data}";
        bags.Add(text);
        semaphore.Release();
        Console.WriteLine(text);
    }

    public static void Error(string data)
    {
        string text = $"[{DateTime.Now}][Error]{data}";
        bags.Add(text);
        semaphore.Release();
        Console.WriteLine(text);
    }

    public static void Error(string data, Exception e)
    {
        string text = $"[{DateTime.Now}][Error]{data}{Environment.NewLine}{e}";
        bags.Add(text);
        semaphore.Release();
        Console.WriteLine(text);
    }
}
