using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using ColorMC.Core;
using System;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class ColorSel : INotifyPropertyChanged
{
    public static readonly IBrush AppBackColor = Brush.Parse("#FFFFFFFF");
    public static readonly IBrush AppBackColor1 = Brush.Parse("#11FFFFFF");

    public static IBrush MainColor { get; private set; } = Brush.Parse("#FF5ABED6");
    public static IBrush BackColor { get; private set; } = Brush.Parse("#FFF4F4F5");
    public static IBrush Back1Color { get; private set; } = Brush.Parse("#88FFFFFF");
    public static IBrush ButtonFont { get; private set; } = Brush.Parse("#FFFFFFFF");
    public static IBrush FontColor { get; private set; } = Brush.Parse("#FF000000");
    public static IBrush MotdColor { get; private set; } = Brush.Parse("#FFFFFFFF");
    public static IBrush MotdBackColor { get; private set; } = Brush.Parse("#FF000000");

    public static ColorSel Instance { get; set; } = new ColorSel();

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    public void Load()
    {
        try
        {
            if (GuiConfigUtils.Config.RGB == true)
            {
                EnableRGB();
            }
            else
            {
                DisableRGB();
                MainColor = Brush.Parse(GuiConfigUtils.Config.ColorMain);
                BackColor = Brush.Parse(GuiConfigUtils.Config.ColorBack);
                Back1Color = Brush.Parse(GuiConfigUtils.Config.ColorTranBack);
                ButtonFont = Brush.Parse(GuiConfigUtils.Config.ColorFont1);
                FontColor = Brush.Parse(GuiConfigUtils.Config.ColorFont2);

                MotdColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdColor);
                MotdBackColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdBackColor);

                Reload();
            }
        }
        catch (Exception e)
        {
            Logs.Error(Localizer.Instance["Error11"], e);
        }
    }

    private readonly Thread timer;
    private bool rbg;
    private bool run;
    private double rbg_s = 1;
    private double rbg_v = 1;

    public ColorSel()
    {
        timer = new(Tick)
        {
            Name = "ColorMC-RGB"
        };
        run = true;
        timer.Start();
    }

    public void Stop()
    {
        run = false;
    }

    public void EnableRGB()
    {
        rbg = true;

        rbg_s = (double)GuiConfigUtils.Config.RGBS / 100;
        rbg_v = (double)GuiConfigUtils.Config.RGBV / 100;

        semaphore.Release();
    }

    public void DisableRGB()
    {
        rbg = false;
    }

    private int now;
    private IBrush Color = MainColor;
    private IBrush Color1 = FontColor;
    private Semaphore semaphore = new(0, 2);

    private void Tick(object? obj)
    {
        while (run)
        {
            semaphore.WaitOne();
            while (rbg)
            {
                now += 1;
                now %= 360;
                var temp = HsvColor.ToRgb(now, rbg_s, rbg_v);
                Color = new ImmutableSolidColorBrush(temp);
                if (rbg_v >= 0.8)
                {
                    if (now == 190)
                    {
                        Color1 = Brush.Parse("#FFFFFFFF");
                    }
                    else if (now == 10)
                    {
                        Color1 = Brush.Parse("#FF000000");
                    }
                }
                else
                {
                    Color1 = Brush.Parse("#FFFFFFFF");
                }

                Dispatcher.UIThread.InvokeAsync(Reload).Wait();

                Thread.Sleep(20);
            }
        }
    }

    public IBrush this[string key]
    {
        get
        {
            if (key == "Main")
                return rbg ? Color : MainColor;
            else if (key == "Back")
                return BackColor;
            else if (key == "TranBack")
                return Back1Color;
            else if (key == "Font")
                return FontColor;
            else if (key == "ButtonFont")
                return rbg ? Color1 : ButtonFont;
            else if (key == "Motd")
                return MotdColor;
            else if (key == "MotdBack")
                return MotdBackColor;

            return Brushes.White;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}
