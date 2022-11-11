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
        Writer.WriteLine($"[{DateTime.Now}][Info]{data}");
    }

    public static void Warn(string data)
    {
        Writer.WriteLine($"[{DateTime.Now}][Warn]{data}");
    }

    public static void Error(string data)
    {
        Writer.WriteLine($"[{DateTime.Now}][Error]{data}");
    }

    public static void Error(string data, Exception e)
    {
        Writer.WriteLine($"[{DateTime.Now}][Error]{data}{Environment.NewLine}{e}");
    }
}
