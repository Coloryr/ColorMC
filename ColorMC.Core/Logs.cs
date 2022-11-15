namespace ColorMC.Core;

public static class Logs
{
    private static string Local;
    private static StreamWriter Writer;

    public static void Init(string dir)
    {
        Local = dir + "logs.log";

        try
        {
            Writer = File.AppendText(Local);
            Writer.AutoFlush = true;
        }
        catch (Exception e)
        {
            CoreMain.OnError?.Invoke("日志文件初始化错误", e, true);
        }
    }

    public static void Info(string data)
    {
        string text = $"[{DateTime.Now}][Info]{data}";
        Writer.WriteLine(text);
        Console.WriteLine(text);
    }

    public static void Warn(string data)
    {
        string text = $"[{DateTime.Now}][Warn]{data}";
        Writer.WriteLine(text);
        Console.WriteLine(text);
    }

    public static void Error(string data)
    {
        string text = $"[{DateTime.Now}][Error]{data}";
        Writer.WriteLine(text);
        Console.WriteLine(text);
    }

    public static void Error(string data, Exception e)
    {
        string text = $"[{DateTime.Now}][Error]{data}{Environment.NewLine}{e}";
        Writer.WriteLine(text);
        Console.WriteLine(text);
    }
}
