using System;
using System.Threading;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 界面RGB模式
/// </summary>
public static class RgbColorUtils
{
    private static readonly Thread t_tick = new(Tick)
    {
        Name = "ColorMC RGB Thread",
        IsBackground = true
    };

    private static bool s_rgb;
    private static bool s_run = true;
    private static double s_rgbS = 1;
    private static double s_rgbV = 1;
    private static int s_now;
    private static readonly Semaphore s_semaphore = new(0, 2);

    private static IBrush s_color;
    private static IBrush s_color1;

    public static event Action? ColorChanged;

    static RgbColorUtils()
    {
        t_tick.Start();
        App.OnClose += App_OnClose;
    }

    public static void Load()
    {
        if (GuiConfigUtils.Config.RGB == true)
        {
            EnableRGB();
        }
        else
        {
            DisableRGB();
        }
    }

    /// <summary>
    /// 是否启用
    /// </summary>
    /// <returns></returns>
    public static bool IsEnable()
    {
        return s_rgb;
    }

    /// <summary>
    /// 获取当前颜色
    /// </summary>
    /// <returns></returns>
    public static IBrush GetColor()
    {
        return s_color;
    }

    public static IBrush GetFontColor()
    {
        return s_color1;
    }

    private static void Tick(object? obj)
    {
        while (s_run)
        {
            s_semaphore.WaitOne();
            while (s_rgb)
            {
                s_now += 1;
                s_now %= 360;
                var temp = HsvColor.ToRgb(s_now, s_rgbS, s_rgbV);
                s_color = new ImmutableSolidColorBrush(temp);
                if (s_rgbV >= 0.8)
                {
                    if (s_now == 190)
                    {
                        s_color1 = Brush.Parse("#FFFFFFFF");
                    }
                    else if (s_now == 10)
                    {
                        s_color1 = Brush.Parse("#FF000000");
                    }
                }
                else
                {
                    s_color1 = Brush.Parse("#FFFFFFFF");
                }

                ColorChanged?.Invoke();

                Thread.Sleep(20);
            }
        }
    }

    private static void App_OnClose()
    {
        s_run = false;
    }

    /// <summary>
    /// 启用RGB模式
    /// </summary>
    public static void EnableRGB()
    {
        if (s_rgb)
            return;

        s_rgb = true;

        s_rgbS = (double)GuiConfigUtils.Config.RGBS / 100;
        s_rgbV = (double)GuiConfigUtils.Config.RGBV / 100;

        s_semaphore.Release();
    }

    /// <summary>
    /// 关闭RGB模式
    /// </summary>
    public static void DisableRGB()
    {
        s_rgb = false;
    }
}
