using System;

namespace ColorMC.Gui.UI;

#if Phone

using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Timers;

/// <summary>
/// 长按处理
/// </summary>
public static class LongPressed
{
    private static readonly Timer t_timer;

    private static Action? s_action;
    private static int s_count;

    static LongPressed()
    {
        t_timer = new();
        t_timer.BeginInit();
        t_timer.AutoReset = false;
        t_timer.Elapsed += Timer_Elapsed;
        t_timer.Interval = 500;
        t_timer.EndInit();

        App.OnClose += App_OnClose;
    }

    /// <summary>
    /// 开始一个长按
    /// </summary>
    /// <param name="action">运行</param>
    public static void Pressed(Action action)
    {
        s_action = action;

        t_timer.Start();
    }

    private static void App_OnClose()
    {
        s_action = null;
        t_timer.Dispose();
    }

    private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        s_count++;
        if (s_count >= 4)
        {
            Dispatcher.UIThread.Post(() =>
            {
                s_action?.Invoke();
            });
        }
    }

    /// <summary>
    /// 结束一个长按
    /// </summary>
    public static void Released()
    {
        if (s_count >= 1)
        {
            s_action?.Invoke();
        }
        Cancel();
    }

    public static void Cancel()
    {
        s_action = null;
        t_timer.Stop();
    }
}
#else
public static class LongPressed
{
    public static void Pressed(Action _)
    {

    }
    public static void Released()
    {

    }
    public static void Cancel()
    {

    }
}
#endif