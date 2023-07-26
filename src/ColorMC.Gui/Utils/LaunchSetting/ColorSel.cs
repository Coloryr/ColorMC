using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core.Utils;
using System;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class ColorSel : INotifyPropertyChanged
{
    public static readonly IBrush AppLightBackColor = Brush.Parse("#FFF3F3F3");
    public static readonly IBrush AppLightBackColor1 = Brush.Parse("#EEEEEEEE");
    public static readonly IBrush AppLightBackColor2 = Brush.Parse("#11FFFFFF");
    public static readonly IBrush AppLightBackColor3 = Brush.Parse("#EEEEEE");

    public static readonly IBrush AppDarkBackColor = Brush.Parse("#FF202020");
    public static readonly IBrush AppDarkBackColor1 = Brush.Parse("#EE202020");
    public static readonly IBrush AppDarkBackColor2 = Brush.Parse("#11202020");
    public static readonly IBrush AppDarkBackColor3 = Brush.Parse("#222222");

    public const string MainColorStr = "#FF5ABED6";

    public const string BackLigthColorStr = "#FFF4F4F5";
    public const string Back1LigthColorStr = "#62FFFFFF";
    public const string ButtonLightFontStr = "#FFFFFFFF";
    public const string FontLigthColorStr = "#FF000000";

    public const string BackDarkColorStr = "#FF202020";
    public const string Back1DarkColorStr = "#46202020";
    public const string ButtonDarkFontStr = "#FF202020";
    public const string FontDarkColorStr = "#FFE9E9E9";

    public static IBrush MainColor { get; private set; } = Brush.Parse(MainColorStr);
    public static IBrush BackColor { get; private set; } = Brush.Parse(BackLigthColorStr);
    public static IBrush Back1Color { get; private set; } = Brush.Parse(Back1LigthColorStr);
    public static IBrush ButtonFont { get; private set; } = Brush.Parse(ButtonLightFontStr);
    public static IBrush FontColor { get; private set; } = Brush.Parse(FontLigthColorStr);
    public static IBrush MotdColor { get; private set; } = Brush.Parse("#FFFFFFFF");
    public static IBrush MotdBackColor { get; private set; } = Brush.Parse("#FF000000");
    public static IBrush BottomColor { get; private set; } = AppLightBackColor;
    public static IBrush TopBottomColor { get; private set; } = AppLightBackColor1;
    public static IBrush BottomTranColor { get; private set; } = AppLightBackColor2;
    public static IBrush BottomColor1 { get; private set; } = AppLightBackColor3;

    public readonly static ColorSel Instance = new ColorSel();

    public void LoadMotd()
    {
        try
        {
            MotdColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdColor);
            MotdBackColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdBackColor);
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("Gui.Error11"), e);
        }
    }

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
            }
            MainColor = Brush.Parse(GuiConfigUtils.Config.ColorMain);

            var config = App.NowTheme == PlatformThemeVariant.Light ?
                GuiConfigUtils.Config.ColorLight : GuiConfigUtils.Config.ColorDark;

            BackColor = Brush.Parse(config.ColorBack);
            Back1Color = Brush.Parse(config.ColorTranBack);
            ButtonFont = Brush.Parse(config.ColorFont1);
            FontColor = Brush.Parse(config.ColorFont2);

            LoadMotd();

            BottomColor = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor : AppDarkBackColor;

            TopBottomColor = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor1 : AppDarkBackColor1;

            BottomTranColor = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor2 : AppDarkBackColor2;

            BottomColor1 = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor3 : AppDarkBackColor3;

            Reload();
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("Gui.Error11"), e);
        }
    }

    private readonly Thread t_tick;
    private bool _rgb;
    private bool _run;
    private double _rgbS = 1;
    private double _rgbV = 1;

    public ColorSel()
    {
        t_tick = new(Tick)
        {
            Name = "ColorMC_RGB"
        };
        _run = true;
        t_tick.Start();
        App.OnClose += App_OnClose;
    }

    private void App_OnClose()
    {
        _run = false;
    }

    public void EnableRGB()
    {
        if (_rgb)
            return;

        _rgb = true;

        _rgbS = (double)GuiConfigUtils.Config.RGBS / 100;
        _rgbV = (double)GuiConfigUtils.Config.RGBV / 100;

        semaphore.Release();
    }

    public void DisableRGB()
    {
        _rgb = false;
    }

    private int now;
    private IBrush Color = MainColor;
    private IBrush Color1 = FontColor;
    private Semaphore semaphore = new(0, 2);

    private void Tick(object? obj)
    {
        while (_run)
        {
            semaphore.WaitOne();
            while (_rgb)
            {
                now += 1;
                now %= 360;
                var temp = HsvColor.ToRgb(now, _rgbS, _rgbV);
                Color = new ImmutableSolidColorBrush(temp);
                if (_rgbV >= 0.8)
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
                return _rgb ? Color : MainColor;
            else if (key == "Back")
                return BackColor;
            else if (key == "TranBack")
                return Back1Color;
            else if (key == "Font")
                return FontColor;
            else if (key == "ButtonFont")
                return _rgb ? Color1 : ButtonFont;
            else if (key == "Motd")
                return MotdColor;
            else if (key == "MotdBack")
                return MotdBackColor;
            else if (key == "Bottom")
                return BottomColor;
            else if (key == "Bottom1")
                return BottomColor1;
            else if (key == "TopBottom")
                return TopBottomColor;
            else if (key == "BottomTran")
                return BottomTranColor;
            else if (key == "PointIn")
                return MainColor;

            return Brushes.White;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerArrayName));
    }
}
