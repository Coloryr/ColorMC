using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 样式管理
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// 主题色
    /// </summary>
    public const string MainColorStr = "#FF5ABED6";

    //UI更新器
    private static readonly Dictionary<string, List<WeakReference<IObserver<IBrush>>>> s_colorList = [];
    private static readonly Dictionary<string, List<WeakReference<IObserver<Thickness>>>> s_thinkList = [];
    private static readonly Dictionary<string, List<WeakReference<IObserver<object?>>>> s_styleList = [];
    private static readonly List<WeakReference<IObserver<FontFamily>>> s_fontList = [];

    //主题
    private static readonly ThemeObj s_light;
    private static readonly ThemeObj s_dark;

    private static readonly Random s_random = new();

    private static readonly double s_fontTitleSize = 17;

    /// <summary>
    /// 当前主题
    /// </summary>
    public static ThemeObj NowThemeColor { get; private set; }

    //阴影
    private static BoxShadows s_buttonShadow;
    public static readonly BoxShadows BorderShadows = new(BoxShadow.Parse("0 0 3 1 #1A000000"), [BoxShadow.Parse("0 0 5 -1 #1A000000")]);
    public static BoxShadows BorderSelecrShadows { get; private set; }

    private static readonly IBrush[] s_colors = [Brush.Parse("#3b82f6"), Brush.Parse("#22c55e"),
        Brush.Parse("#eab308"), Brush.Parse("#ef4444"), Brush.Parse("#a855f7"),
        Brush.Parse("#ec4899"), Brush.Parse("#14b8a6"), Brush.Parse("#6366f1"),
        Brush.Parse("#f97316"), Brush.Parse("#06b6d4"), Brush.Parse("#84cc16")];

    public static readonly SelfCrossFade CrossFade = new(TimeSpan.FromMilliseconds(300));
    public static readonly SelfPageSlideY SelfPageSlideY = new(TimeSpan.FromMilliseconds(500));

    private static FontFamily s_font = new(FontFamily.DefaultFontFamilyName);

    private static void RgbColor_ColorChanged()
    {
        Dispatcher.UIThread.Invoke(Reload);
    }

    /// <summary>
    /// 当前样式
    /// </summary>
    public static PlatformThemeVariant NowTheme { get; private set; }

    /// <summary>
    /// 初始化主题
    /// </summary>
    public static void Init()
    {
        switch (GuiConfigUtils.Config.ColorType)
        {
            case ColorType.Auto:
                NowTheme = App.ThisApp.PlatformSettings!.GetColorValues().ThemeVariant;
                break;
            case ColorType.Light:
                NowTheme = PlatformThemeVariant.Light;
                break;
            case ColorType.Dark:
                NowTheme = PlatformThemeVariant.Dark;
                break;
        }

        if (NowTheme == PlatformThemeVariant.Light)
        {
            NowThemeColor = s_light;
        }
        else
        {
            NowThemeColor = s_dark;
        }

        LoadColor();
#if !Phone
        LoadFont();
#endif
        RgbColorUtils.Load();
        ColorManager.Load();

        Reload();
        LoadPageSlide();
    }

    /// <summary>
    /// 加载动画效果
    /// </summary>
    public static void LoadPageSlide()
    {
        var style = GuiConfigUtils.Config.Style;
        SelfPageSlideY.Duration = TimeSpan.FromMilliseconds(style.AmTime);
        SelfPageSlideY.Fade = style.AmFade;
    }

    /// <summary>
    /// 加载主题颜色
    /// </summary>
    private static void LoadColor()
    {
        NowThemeColor.MainColor = Brush.Parse(GuiConfigUtils.Config.ColorMain);
        var color = NowThemeColor.MainColor.ToColor();
        var color1 = new Color(255, color.R, color.G, color.B);

        s_buttonShadow = new(new BoxShadow
        {
            Blur = 3,
            Spread = 1,
            Color = color1
        });

        BorderSelecrShadows = new(new BoxShadow
        {
            Blur = 3,
            Spread = 1,
            Color = color1
        });
    }

    /// <summary>
    /// 加载字体
    /// </summary>
    private static void LoadFont()
    {
        if (!GuiConfigUtils.Config.FontDefault
            && !string.IsNullOrWhiteSpace(GuiConfigUtils.Config.FontName)
            && FontManager.Current.SystemFonts.Any(a => a.Name == GuiConfigUtils.Config.FontName)
            && SkiaSharp.SKFontManager.Default.MatchFamily(GuiConfigUtils.Config.FontName) is { } font)
        {
            s_font = new(font.FamilyName);
        }
        else
        {
            s_font = new(ColorMCGui.Font);
        }
    }

    /// <summary>
    /// 获取颜色
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IBrush GetColor(string key)
    {
        if (key == nameof(ThemeObj.WindowBG))
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(NowThemeColor.WindowBG.ToColor(), 0.75);
            }
            else if (GuiConfigUtils.Config.WindowTran)
            {
                return Brushes.Transparent;
            }
            return NowThemeColor.WindowBG;
        }
        else if (key == nameof(ThemeObj.WindowTranColor))
        {
            if (GuiConfigUtils.Config.WindowTran)
            {
                return NowThemeColor.WindowTranColor;
            }
            else if (NowTheme == PlatformThemeVariant.Light)
            {
                return Brushes.White;
            }
            else
            {
                return Brushes.Black;
            }
        }
        else if (key == "WindowBase")
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(NowThemeColor.WindowBG.ToColor(), 0.75);
            }
            return Brushes.Transparent;
        }
        else if (key == nameof(ThemeObj.ItemBG))
        {
            return NowThemeColor.ItemBG;
        }
        else if (key == nameof(ThemeObj.MainGroupBG))
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(NowThemeColor.MainGroupBG.ToColor(), 0.75);
            }
            return NowThemeColor.MainGroupBG;
        }
        else if (key == nameof(ThemeObj.MainGroupBorder))
        {
            if (GuiConfigUtils.Config.WindowTran && ImageManager.BackBitmap == null)
            {
                return NowThemeColor.MainGroupBorder;
            }

            return Brushes.Transparent;
        }
        else if (key == "MainGroupItemBG")
        {
            if (ImageManager.BackBitmap != null)
            {
                return new SolidColorBrush(NowThemeColor.MainGroupBG.ToColor(), 0.75);
            }
            return Brushes.Transparent;
        }
        else if (key == nameof(ThemeObj.ProgressBarBG))
        {
            return NowThemeColor.ProgressBarBG;
        }
        else if (key == nameof(ThemeObj.GameItemBG))
        {
            return NowThemeColor.GameItemBG;
        }
        else if (key == nameof(ThemeObj.TopViewBG))
        {
            return NowThemeColor.TopViewBG;
        }
        else if (key == nameof(ThemeObj.AllBorder))
        {
            return NowThemeColor.AllBorder;
        }
        else if (key == nameof(ThemeObj.ButtonBG))
        {
            return NowThemeColor.ButtonBG;
        }
        else if (key == nameof(ThemeObj.ButtonOver))
        {
            return NowThemeColor.ButtonOver;
        }
        else if (key == nameof(ThemeObj.ButtonBorder))
        {
            return NowThemeColor.ButtonBorder;
        }
        else if (key == nameof(ThemeObj.MainColor))
        {
            return RgbColorUtils.IsEnable() ? RgbColorUtils.GetColor() : NowThemeColor.MainColor;
        }
        else if (key == nameof(ThemeObj.FontColor))
        {
            return NowThemeColor.FontColor;
        }
        else if (key == nameof(ThemeObj.TopBGColor))
        {
            return NowThemeColor.TopBGColor;
        }
        else if (key == nameof(ThemeObj.TopGridColor))
        {
            return NowThemeColor.TopGridColor;
        }
        else if (key == nameof(ThemeObj.OverBGColor))
        {
            return NowThemeColor.OverBGColor;
        }
        else if (key == nameof(ThemeObj.OverBrushColor))
        {
            return NowThemeColor.OverBrushColor;
        }
        else if (key == nameof(ThemeObj.SelectItemBG))
        {
            return NowThemeColor.SelectItemBG;
        }
        else if (key == nameof(ThemeObj.SelectItemOver))
        {
            return NowThemeColor.SelectItemOver;
        }
        else if (key == nameof(ThemeObj.MenuBG))
        {
            return NowThemeColor.MenuBG;
        }
        else if (key == "RandomColor")
        {
            return s_colors[s_random.Next(s_colors.Length)];
        }

        return Brushes.Transparent;
    }

    /// <summary>
    /// 获取样式
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static object? GetStyle(string key)
    {
        if (key == "FontTitle")
        {
            return s_fontTitleSize;
        }
        else if (key == "ButtonTopBoxShadow")
        {
            return s_buttonShadow;
        }
        return null;
    }

    /// <summary>
    /// 添加UI字体绑定
    /// </summary>
    /// <param name="observer"></param>
    /// <returns></returns>
    public static IDisposable AddFont(IObserver<FontFamily> observer)
    {
        s_fontList.Add(new WeakReference<IObserver<FontFamily>>(observer));
        observer.OnNext(s_font);
        return new UnsubscribeFont(s_fontList, observer);
    }

    /// <summary>
    /// 添加UI样式绑定
    /// </summary>
    /// <param name="key"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    public static IDisposable AddStyle(string key, IObserver<object?> observer)
    {
        if (s_styleList.TryGetValue(key, out var list))
        {
            list.Add(new(observer));
        }
        else
        {
            list = [new(observer)];
            s_styleList.Add(key, list);
        }
        var value = GetStyle(key);
        observer.OnNext(value);
        return new UnsubscribeStyle(list, observer);
    }

    /// <summary>
    /// 添加UI边框绑定
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static Thickness GetThick(string key)
    {
        if (key == "Border")
        {
            return GuiConfigUtils.Config.WindowTran && ImageManager.BackBitmap == null ? new(1) : new(0);
        }

        return new(0);
    }

    /// <summary>
    /// 添加UI颜色绑定
    /// </summary>
    /// <param name="key"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    public static IDisposable Add(string key, IObserver<IBrush> observer)
    {
        if (s_colorList.TryGetValue(key, out var list))
        {
            list.Add(new(observer));
        }
        else
        {
            list = [new(observer)];
            s_colorList.Add(key, list);
        }
        var value = GetColor(key);
        observer.OnNext(value);
        return new UnsubscribeColor(list, observer);
    }

    /// <summary>
    /// 添加UI边框绑定
    /// </summary>
    /// <param name="key"></param>
    /// <param name="observer"></param>
    /// <returns></returns>
    public static IDisposable Add(string key, IObserver<Thickness> observer)
    {
        if (s_thinkList.TryGetValue(key, out var list))
        {
            list.Add(new(observer));
        }
        else
        {
            list = [new(observer)];
            s_thinkList.Add(key, list);
        }
        var value = GetThick(key);
        observer.OnNext(value);
        return new UnsubscribeThick(list, observer);
    }

    /// <summary>
    /// 清理UI绑定
    /// </summary>
    public static void Remove()
    {
        ColorManager.Remove();

        foreach (var item in s_colorList.Values)
        {
            foreach (var item1 in item.ToArray())
            {
                if (!item1.TryGetTarget(out _))
                {
                    item.Remove(item1);
                }
            }
        }
        foreach (var item in s_thinkList.Values)
        {
            foreach (var item1 in item.ToArray())
            {
                if (!item1.TryGetTarget(out _))
                {
                    item.Remove(item1);
                }
            }
        }

        foreach (var item in s_styleList.Values)
        {
            foreach (var item1 in item.ToArray())
            {
                if (!item1.TryGetTarget(out _))
                {
                    item.Remove(item1);
                }
            }
        }

        foreach (var item in s_fontList.ToArray())
        {
            if (!item.TryGetTarget(out _))
            {
                s_fontList.Remove(item);
            }
        }
    }

    /// <summary>
    /// 重载所有样式
    /// </summary>
    private static void Reload()
    {
        foreach (var item in s_colorList)
        {
            if (item.Key.StartsWith("Random"))
            {
                foreach (var item1 in item.Value)
                {
                    if (item1.TryGetTarget(out var target))
                    {
                        target.OnNext(GetColor(item.Key));
                    }
                }
            }
            else
            {
                var value = GetColor(item.Key);
                foreach (var item1 in item.Value)
                {
                    if (item1.TryGetTarget(out var target))
                    {
                        target.OnNext(value);
                    }
                }
            }
        }
        foreach (var item in s_thinkList)
        {
            var value = GetThick(item.Key);
            foreach (var item1 in item.Value)
            {
                if (item1.TryGetTarget(out var target))
                {
                    target.OnNext(value);
                }
            }
        }

        foreach (var item in s_styleList)
        {
            var value = GetStyle(item.Key);
            foreach (var item1 in item.Value)
            {
                if (item1.TryGetTarget(out var target))
                {
                    target.OnNext(value);
                }
            }
        }

        foreach (var item in s_fontList)
        {
            if (item.TryGetTarget(out var target))
            {
                target.OnNext(s_font);
            }
        }
    }

    static ThemeManager()
    {
        RgbColorUtils.ColorChanged += RgbColor_ColorChanged;

        s_light = new()
        {
            MainColor = Brush.Parse(MainColorStr),
            FontColor = Brush.Parse("#FF000000"),
            WindowBG = Brush.Parse("#65CCCCCC"),
            WindowTranColor = Brush.Parse("#80FFFFFF"),
            ProgressBarBG = Brush.Parse("#FFe4e4e7"),
            MainGroupBG = Brush.Parse("#FFd4d4d8"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#CFFFFFFF"),
            GameItemBG = Brush.Parse("#FFF2F2F2"),
            TopViewBG = Brush.Parse("#886D6D6D"),
            AllBorder = Brush.Parse("#FFe5e7eb"),
            ButtonBG = Brush.Parse("#FFFFFFFF"),
            ButtonOver = Brush.Parse("#FFFEFEFE"),
            ButtonBorder = Brush.Parse("#FFD4D4D8"),
            TopBGColor = Brush.Parse("#EFFFFFFF"),
            TopGridColor = Brush.Parse("#FFFFFFFF"),
            OverBGColor = Brush.Parse("#FFFFFFFF"),
            OverBrushColor = Brush.Parse("#FFe5e5e5"),
            SelectItemBG = Brush.Parse("#D1E0E0E0"),
            SelectItemOver = Brush.Parse("#FFCCCCCC"),
            MenuBG = Brush.Parse("#FFF4F4F5"),
        };

        s_dark = new()
        {
            MainColor = Brush.Parse(MainColorStr),
            FontColor = Brush.Parse("#FFFFFFFF"),
            WindowBG = Brush.Parse("#22141417"),
            WindowTranColor = Brush.Parse("#80202020"),
            ProgressBarBG = Brush.Parse("#FF3f3f46"),
            MainGroupBG = Brush.Parse("#FF27272a"),
            MainGroupBorder = Brush.Parse("#FFE0E0E0"),
            ItemBG = Brush.Parse("#CF27272a"),
            GameItemBG = Brush.Parse("#FFc7c7cb"),
            TopViewBG = Brush.Parse("#886D6D6D"),
            AllBorder = Brush.Parse("#FFe5e7eb"),
            ButtonBG = Brush.Parse("#FF000000"),
            ButtonOver = Brush.Parse("#FF141414"),
            ButtonBorder = Brush.Parse("#FFD4D4D8"),
            TopBGColor = Brush.Parse("#EF000000"),
            TopGridColor = Brush.Parse("#FF202020"),
            OverBGColor = Brush.Parse("#FF000000"),
            OverBrushColor = Brush.Parse("#FF1d1d1d"),
            SelectItemBG = Brush.Parse("#FF353535"),
            SelectItemOver = Brush.Parse("#FF454545"),
            MenuBG = Brush.Parse("#FF2c2c2c"),
        };
    }
}
