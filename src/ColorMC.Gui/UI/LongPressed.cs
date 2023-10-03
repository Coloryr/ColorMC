using System;
using System.Timers;

namespace ColorMC.Gui.UI;

public static class LongPressed
{
    private static bool s_init = false;
    private static Timer t_timer;

    private static Action? s_action;

    public static void Pressed(Action action)
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

        s_action = action;
        t_timer.Start();
    }

    private static void App_OnClose()
    {
        t_timer.Dispose();
    }

    private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        s_action?.Invoke();
    }

    public static void Released()
    {
        if (s_init)
        {
            s_action = null;
            t_timer.Stop();
        }
    }
}
