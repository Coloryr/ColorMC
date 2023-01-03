using System.Collections.Concurrent;

namespace ColorMC.Cmd;

public class ProgressBar : IDisposable
{
    private readonly Thread timer;
    private readonly int Count;
    private int Top;
    private readonly long[] AllSize;
    private readonly long[] Now;
    private readonly string[] Names;
    private bool Run;

    private readonly ConcurrentBag<(int, string)> Dones = new();

    public ProgressBar(int count)
    {
        Top = Console.CursorTop;
        Count = count;
        AllSize = new long[count];
        Now = new long[count];
        Names = new string[count];
        for (int a = 0; a < count; a++)
        {
            Console.WriteLine("> INIT");
        }
        timer = new Thread(Tick);
        Run = true;
        timer.Start();
    }

    public void Tick(object? state)
    {
        while (Run)
        {
            while (Dones.TryTake(out var item))
            {
                AllSize[item.Item1] = 0;
                Now[item.Item1] = 0;
                Names[item.Item1] = null;
            }
            for (int a = 0; a < Count; a++)
            {
                Console.SetCursorPosition(0, Top + a);
                if (string.IsNullOrWhiteSpace(Names[a]))
                {
                    Console.Write("> IDLE" + new string(' ', Console.WindowWidth - 6));
                }
                else
                {
                    int p = (int)(AllSize[a] != 0 ? (double)Now[a] / AllSize[a] * 10 : 0);
                    Console.Write($"> {Names[a]} [{new string('*', p)}{new string('-', 10 - p)}] {Now[a]}/{AllSize[a]}");
                }
            }
            Thread.Sleep(100);
        }
    }

    public void Done(int index, string a)
    {
        Dones.Add((index, a));
    }

    public void SetValue(int index, long value)
    {
        Now[index] = value;
    }

    public void SetAllSize(int index, long value)
    {
        AllSize[index] = value;
    }

    public void SetName(int index, string a)
    {
        Names[index] = a;
    }

    public void Dispose()
    {
        Run = false;
        timer.Join();
        Console.SetCursorPosition(0, Top);
        for (int a = 0; a < Count; a++)
        {
            Console.WriteLine(new string(' ', Console.WindowWidth));
        }
    }
}
