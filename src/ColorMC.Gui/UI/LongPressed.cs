using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System;
using System.Timers;

namespace ColorMC.Gui.UI;

/// <summary>
/// 长按处理
/// </summary>
public static class LongPressed
{
    private static bool s_init = false;
    private static Timer t_timer;

    private static Action? s_action;

    /// <summary>
    /// 开始一个长按
    /// </summary>
    /// <param name="action">运行</param>
    public static void Pressed(Action action)
    {
        s_action = action;
        if (SystemInfo.Os != OsType.Android)
        {
            if (!s_init)
            {
                s_init = true;
                t_timer = new();
                t_timer.BeginInit();
                t_timer.AutoReset = false;
                t_timer.Elapsed += Timer_Elapsed;
                t_timer.Interval = 1000;
                t_timer.EndInit();

                App.OnClose += App_OnClose;
            }

            t_timer.Start();
        }
    }

    private static void App_OnClose()
    {
        s_action = null;
        t_timer.Dispose();
    }

    private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        s_action?.Invoke();
    }

    /// <summary>
    /// 结束一个长按
    /// </summary>
    public static void Released()
    {
        if (SystemInfo.Os == OsType.Android)
        {
            s_action?.Invoke();
        }
        else if (s_init)
        {
            s_action = null;

            t_timer.Stop();
        }
    }
}
