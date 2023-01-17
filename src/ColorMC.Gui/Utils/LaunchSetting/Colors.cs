using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using ColorMC.Core;
using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class Colors : INotifyPropertyChanged
{
    public static readonly IBrush AppBackColor = Brush.Parse("#FFFFFFFF");
    public static readonly IBrush AppBackColor1 = Brush.Parse("#11FFFFFF");

    public static IBrush MainColor = Brush.Parse("#FF5ABED6");
    public static IBrush BackColor = Brush.Parse("#FFF4F4F5");
    public static IBrush Back1Color = Brush.Parse("#88FFFFFF");

    public static Colors Instance { get; set; } = new Colors();

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    public static void Load()
    {
        try
        {
            if (GuiConfigUtils.Config.RGB == true)
            {
                Instance.EnableRGB();
            }
            else
            {
                Instance.DisableRGB();
                MainColor = Brush.Parse(GuiConfigUtils.Config.ColorMain);
                BackColor = Brush.Parse(GuiConfigUtils.Config.ColorBack);
                Back1Color = Brush.Parse(GuiConfigUtils.Config.ColorTranBack);

                Instance.Reload();
            }
        }
        catch (Exception e)
        {
            Logs.Error("颜色数据读取失败", e);
        }
    }

    private Thread timer;
    private bool rbg;
    private bool run;

    public Colors()
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
    }

    public void DisableRGB()
    {
        rbg = false;
    }

    private int now;
    private IBrush Color = MainColor;

    private void Tick(object? obj)
    {
        while (run)
        {
            if (rbg)
            {
                now += 1;
                now %= 360;
                Color = new ImmutableSolidColorBrush(HsvColor.ToRgb(now, 0.8, 1));
                Dispatcher.UIThread.InvokeAsync(Reload).Wait();
            }
            Thread.Sleep(20);
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
