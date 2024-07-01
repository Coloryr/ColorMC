using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;

namespace ColorMC.Gui.Utils;

public static class RgbColor
{
    private static readonly Thread t_tick = new(Tick)
    {
        Name = "ColorMC_RGB"
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

    static RgbColor()
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

    public static bool IsEnable()
    {
        return s_rgb;
    }

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
