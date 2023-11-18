using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 颜色设定
/// </summary>
public static class ColorSel
{
    public static readonly IBrush AppLightBackColor = Brush.Parse("#FFF3F3F3");
    public static readonly IBrush AppLightBackColor1 = Brush.Parse("#AA989898");
    public static readonly IBrush AppLightBackColor2 = Brush.Parse("#11FFFFFF");
    public static readonly IBrush AppLightBackColor3 = Brush.Parse("#EEEEEE");
    public static readonly IBrush AppLightBackColor4 = Brush.Parse("#CCCCCC");
    public static readonly IBrush AppLightBackColor5 = Brush.Parse("#22FFFFFF");
    public static readonly IBrush AppLightBackColor6 = Brush.Parse("#FFDDDDDD");
    public static readonly IBrush AppLightBackColor7 = Brush.Parse("#DDFFFFFF");
    public static readonly IBrush AppLightBackColor8 = Brush.Parse("#FFDDDDDD"); //button boder

    public static readonly IBrush AppDarkBackColor = Brush.Parse("#FF202020");
    public static readonly IBrush AppDarkBackColor1 = Brush.Parse("#CC3A3A3A");
    public static readonly IBrush AppDarkBackColor2 = Brush.Parse("#11202020");
    public static readonly IBrush AppDarkBackColor3 = Brush.Parse("#222222");
    public static readonly IBrush AppDarkBackColor4 = Brush.Parse("#888888");
    public static readonly IBrush AppDarkBackColor5 = Brush.Parse("#AA000000");
    public static readonly IBrush AppDarkBackColor6 = Brush.Parse("#FF444444");
    public static readonly IBrush AppDarkBackColor7 = Brush.Parse("#EE000000");
    public static readonly IBrush AppDarkBackColor8 = Brush.Parse("#FFEEEEEE"); //button boder

    public const string MainColorStr = "#FF5ABED6";

    public const string BackLigthColorStr = "#FFF4F4F5";
    public const string Back1LigthColorStr = "#66FFFFFF";
    public const string ButtonLightFontStr = "#FFFFFFFF";
    public const string FontLigthColorStr = "#FF000000";

    public const string BackDarkColorStr = "#FF202020";
    public const string Back1DarkColorStr = "#46202020";
    public const string ButtonDarkFontStr = "#FF000000";
    public const string FontDarkColorStr = "#FFE9E9E9";

    public const string GroupLightColorStr = "#CCfbfbfb";
    public const string GroupDarkColorStr = "#CC000000";

    public const string GroupLightColor1Str = "#FFe5e5e5";
    public const string GroupDarkColor1Str = "#FF1d1d1d";

    public static IBrush MainColor { get; private set; } = Brush.Parse(MainColorStr);
    public static IBrush BackColor { get; private set; } = Brush.Parse(BackLigthColorStr);
    public static IBrush Back1Color { get; private set; } = Brush.Parse(Back1LigthColorStr);
    public static IBrush Back2Color { get; private set; } = AppLightBackColor6;
    public static IBrush ButtonFont { get; private set; } = Brush.Parse(ButtonLightFontStr);
    public static IBrush FontColor { get; private set; } = Brush.Parse(FontLigthColorStr);
    public static IBrush MotdColor { get; private set; } = Brush.Parse("#FFFFFFFF");
    public static IBrush MotdBackColor { get; private set; } = Brush.Parse("#FF000000");
    public static IBrush BottomColor { get; private set; } = AppLightBackColor;
    public static IBrush TopBottomColor { get; private set; } = AppLightBackColor1;
    public static IBrush BottomTranColor { get; private set; } = AppLightBackColor2;
    public static IBrush BottomColor1 { get; private set; } = AppLightBackColor3;
    public static IBrush BottomColor2 { get; private set; } = AppLightBackColor4;
    public static IBrush BGColor { get; private set; } = AppLightBackColor5;
    public static IBrush BGColor1 { get; private set; } = AppLightBackColor7;
    public static IBrush GroupBackColor { get; private set; } = Brush.Parse(GroupLightColorStr);
    public static IBrush GroupBackColor1 { get; private set; } = Brush.Parse(GroupLightColor1Str);
    public static IBrush GroupBackColor2 { get; private set; } = Brush.Parse(GroupLightColor1Str);
    public static IBrush ButtonBorder { get; private set; } = AppLightBackColor8;


    private static int s_now;
    private static IBrush s_color = MainColor;
    private static IBrush s_color1 = FontColor;
    private static readonly Semaphore s_semaphore = new(0, 2);
    private static readonly Dictionary<string, List<IObserver<IBrush>>> s_colorList = [];

    private static readonly Thread t_tick = new(Tick)
    {
        Name = "ColorMC_RGB"
    };
    private static bool s_rgb;
    private static bool s_run = true;
    private static double s_rgbS = 1;
    private static double s_rgbV = 1;

    static ColorSel()
    {
        t_tick.Start();
        App.OnClose += App_OnClose;
    }

    /// <summary>
    /// 加载颜色
    /// </summary>
    public static void Load()
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

            BottomColor = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor : AppDarkBackColor;

            TopBottomColor = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor1 : AppDarkBackColor1;

            BottomTranColor = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor2 : AppDarkBackColor2;

            BottomColor1 = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor3 : AppDarkBackColor3;

            BottomColor2 = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor4 : AppDarkBackColor4;

            Back2Color = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor6 : AppDarkBackColor6;

            BGColor = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor5 : AppDarkBackColor5;

            BGColor1 = App.NowTheme == PlatformThemeVariant.Light
               ? AppLightBackColor7 : AppDarkBackColor7;

            GroupBackColor = App.NowTheme == PlatformThemeVariant.Light
                ? Brush.Parse(GroupLightColorStr) : Brush.Parse(GroupDarkColorStr);

            ButtonBorder = App.NowTheme == PlatformThemeVariant.Light
                ? AppLightBackColor8 : AppDarkBackColor8;

            if (App.NowTheme == PlatformThemeVariant.Light)
            {
                GroupBackColor2 = Brush.Parse(GroupLightColor1Str);
            }
            else
            {
                GroupBackColor2 = Brush.Parse(GroupDarkColor1Str);
            }
            if (App.BackBitmap != null)
            {
                GroupBackColor1 = Brushes.Transparent;
            }
            else
            {
                GroupBackColor1 = GroupBackColor2;
            }

            MotdColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdColor);
            MotdBackColor = Brush.Parse(GuiConfigUtils.Config.ServerCustom.MotdBackColor);

            Reload();
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("Gui.Error11"), e);
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

                Dispatcher.UIThread.InvokeAsync(Reload).Wait();

                Thread.Sleep(20);
            }
        }
    }

    public static IDisposable Add(string key, IObserver<IBrush> observer)
    {
        if (s_colorList.TryGetValue(key, out var list))
        {
            list.Add(observer);
        }
        else
        {
            list = [observer];
            s_colorList.Add(key, list);
        }
        var value = GetColor(key);
        observer.OnNext(value);
        return new Unsubscribe(list, observer);
    }

    private class Unsubscribe(List<IObserver<IBrush>> observers, IObserver<IBrush> observer) : IDisposable
    {
        public void Dispose()
        {
            observers.Remove(observer);
        }
    }

    public static void Reload()
    {
        foreach (var item in s_colorList)
        {
            var value = GetColor(item.Key);
            foreach (var item1 in item.Value)
            {
                item1.OnNext(value);
            }
        }
    }

    private static IBrush GetColor(string key)
    {
        if (key == "Main")
            return s_rgb ? s_color : MainColor;
        else if (key == "Back")
            return BackColor;
        else if (key == "TranBack")
            return Back1Color;
        else if (key == "Font")
            return FontColor;
        else if (key == "ButtonFont")
            return s_rgb ? s_color1 : ButtonFont;
        else if (key == "Motd")
            return MotdColor;
        else if (key == "MotdBack")
            return MotdBackColor;
        else if (key == "Bottom")
            return BottomColor;
        else if (key == "Bottom1")
            return BottomColor1;
        else if (key == "Bottom2")
            return BottomColor2;
        else if (key == "TopBottom")
            return TopBottomColor;
        else if (key == "BottomTran")
            return BottomTranColor;
        else if (key == "PointIn")
            return MainColor;
        else if (key == "GroupBack")
            return GroupBackColor;
        else if (key == "GroupColor")
            return GroupBackColor1;
        else if (key == "GroupColor1")
            return GroupBackColor2;
        else if (key == "BG")
            return BGColor;
        else if (key == "BG1")
            return BGColor1;
        else if (key == "Back1")
            return Back2Color;
        else if (key == "ButtonBorder")
            return ButtonBorder;

        return Brushes.White;
    }
}
