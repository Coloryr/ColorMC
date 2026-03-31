using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    /// <summary>
    /// 字体列表
    /// </summary>
    public ObservableCollection<FontDisplayModel> FontList { get; init; } = [];
    /// <summary>
    /// 透明类型列表
    /// </summary>
    public string[] TranTypeList { get; init; } = LangUtils.GetWindowTranTypes();
    /// <summary>
    /// 语言列表
    /// </summary>
    public string[] LanguageList { get; init; } = LangUtils.GetLanguages();
    /// <summary>
    /// 位置列表
    /// </summary>
    public string[] PosList { get; init; } = LangUtils.GetPos();

    /// <summary>
    /// 字体
    /// </summary>
    [ObservableProperty]
    public partial FontDisplayModel? FontItem { get; set; }

    /// <summary>
    /// 主要颜色
    /// </summary>
    [ObservableProperty]
    public partial Color MainColor { get; set; }

    /// <summary>
    /// 警告颜色
    /// </summary>
    [ObservableProperty]
    public partial Color WarnColor { get; set; }

    /// <summary>
    /// 错误颜色
    /// </summary>
    [ObservableProperty]
    public partial Color ErrorColor { get; set; }

    /// <summary>
    /// 调试颜色
    /// </summary>
    [ObservableProperty]
    public partial Color DebugColor { get; set; }

    /// <summary>
    /// 是否启用单窗口模式
    /// </summary>
    [ObservableProperty]
    public partial bool WindowMode { get; set; }

    /// <summary>
    /// 是否使用自定义字体
    /// </summary>
    [ObservableProperty]
    public partial bool IsCutsomFont { get; set; }

    /// <summary>
    /// 是否启用背景图分辨率缩放
    /// </summary>
    [ObservableProperty]
    public partial bool EnablePicResize { get; set; }

    /// <summary>
    /// 是否启动自动主题
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoColor { get; set; }

    /// <summary>
    /// 是否启用亮主题
    /// </summary>
    [ObservableProperty]
    public partial bool IsLightColor { get; set; }

    /// <summary>
    /// 是否启用暗主题
    /// </summary>
    [ObservableProperty]
    public partial bool IsDarkColor { get; set; }

    /// <summary>
    /// 是否启用RGB模式
    /// </summary>
    [ObservableProperty]
    public partial bool EnableRGB { get; set; }

    /// <summary>
    /// 是否启用窗口透明
    /// </summary>
    [ObservableProperty]
    public partial bool EnableWindowTran { get; set; }

    /// <summary>
    /// 是否启用动画虚化
    /// </summary>
    [ObservableProperty]
    public partial bool AmFade { get; set; }

    /// <summary>
    /// 是否启用自定义背景图
    /// </summary>
    [ObservableProperty]
    public partial bool EnableBG { get; set; }

    /// <summary>
    /// 是否启用动画
    /// </summary>
    [ObservableProperty]
    public partial bool EnableAm { get; set; }

    /// <summary>
    /// 是否展示Minecraft news
    /// </summary>
    [ObservableProperty]
    public partial bool CardNews { get; set; }

    /// <summary>
    /// 是否展示上次启动
    /// </summary>
    [ObservableProperty]
    public partial bool CardLast { get; set; }

    /// <summary>
    /// 是否展示在线联机
    /// </summary>
    [ObservableProperty]
    public partial bool CardOnline { get; set; }

    /// <summary>
    /// 是否展示幸运方块
    /// </summary>
    [ObservableProperty]
    public partial bool CardBlock { get; set; }

    /// <summary>
    /// 头像旋转
    /// </summary>
    [ObservableProperty]
    public partial int HeadX { get; set; }

    /// <summary>
    /// 头像旋转
    /// </summary>
    [ObservableProperty]
    public partial int HeadY { get; set; }

    /// <summary>
    /// 选中的语言
    /// </summary>
    [ObservableProperty]
    public partial LanguageType Language { get; set; }

    /// <summary>
    /// 背景图虚化
    /// </summary>
    [ObservableProperty]
    public partial int PicEffect { get; set; }

    /// <summary>
    /// 背景图透明
    /// </summary>
    [ObservableProperty]
    public partial int PicTran { get; set; }

    /// <summary>
    /// 背景图缩小分辨率
    /// </summary>
    [ObservableProperty]
    public partial int PicResize { get; set; }

    /// <summary>
    /// 窗口透明模式
    /// </summary>
    [ObservableProperty]
    public partial int WindowTranType { get; set; }

    /// <summary>
    /// RGB
    /// </summary>
    [ObservableProperty]
    public partial int RgbV1 { get; set; }

    /// <summary>
    /// RGB
    /// </summary>
    [ObservableProperty]
    public partial int RgbV2 { get; set; }

    /// <summary>
    /// 动画时间
    /// </summary>
    [ObservableProperty]
    public partial int AmTime { get; set; }

    /// <summary>
    /// 背景图位置
    /// </summary>
    [ObservableProperty]
    public partial string? Pic { get; set; }

    /// <summary>
    /// 头像模式
    /// </summary>
    [ObservableProperty]
    public partial HeadType HeadType { get; set; }

    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _load = true;

    /// <summary>
    /// 图标
    /// </summary>
    public string IconHead
    {
        get
        {
            var random = new Random();
            var index = random.Next(200000);
            if (index == 114514)
            {
                return ImageManager.EmoticonIcon[random.Next(5) + 1];
            }

            return ImageManager.EmoticonIcon[0];
        }
    }

    partial void OnCardBlockChanged(bool value)
    {
        SetCard();
    }

    partial void OnCardLastChanged(bool value)
    {
        SetCard();
    }

    partial void OnCardNewsChanged(bool value)
    {
        SetCard();
    }

    partial void OnCardOnlineChanged(bool value)
    {
        SetCard();
    }

    partial void OnHeadTypeChanged(HeadType value)
    {
        if (_load)
            return;

        ConfigBinding.SetHeadType(value);
    }

    partial void OnHeadXChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetHeadXY(value, HeadY);
    }

    partial void OnHeadYChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetHeadXY(HeadX, value);
    }

    async partial void OnEnableBGChanged(bool value)
    {
        if (_load)
            return;

        await SetPic();
    }

    partial void OnEnableAmChanged(bool value)
    {
        SetAm();
    }

    partial void OnAmFadeChanged(bool value)
    {
        SetAm();
    }

    partial void OnAmTimeChanged(int value)
    {
        SetAm();
    }

    partial void OnMainColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnFontItemChanged(FontDisplayModel? value)
    {
        if (_load || value == null)
            return;

        OnPropertyChanged("Hide");

        ConfigBinding.SetFont(value.FontName, !IsCutsomFont);
    }

    partial void OnIsCutsomFontChanged(bool value)
    {
        if (_load || FontItem == null)
            return;

        ConfigBinding.SetFont(FontItem.FontName, !IsCutsomFont);
    }

    partial void OnEnableWindowTranChanged(bool value)
    {
        SaveWindowSetting();
    }

    partial void OnEnableRGBChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetRgb(value);
    }

    partial void OnRgbV1Changed(int value)
    {
        SetRgb();
    }

    partial void OnRgbV2Changed(int value)
    {
        SetRgb();
    }

    partial void OnWindowModeChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetWindowMode(value);
    }

    partial void OnIsAutoColorChanged(bool value)
    {
        if (_load || !value)
            return;

        ConfigBinding.SetColorType(ColorType.Auto);
    }

    partial void OnIsLightColorChanged(bool value)
    {
        if (_load || !value)
            return;

        ConfigBinding.SetColorType(ColorType.Light);
    }

    partial void OnIsDarkColorChanged(bool value)
    {
        if (_load || !value)
            return;

        ConfigBinding.SetColorType(ColorType.Dark);
    }

    async partial void OnEnablePicResizeChanged(bool value)
    {
        if (_load)
            return;

        if (value)
        {
            var dialog = Window.ShowProgress(LangUtils.Get("SettingWindow.Tab2.Text70"));
            await ConfigBinding.SetBackLimit(value, PicResize);
            Window.CloseDialog(dialog);
        }
    }

    partial void OnLanguageChanged(LanguageType value)
    {
        if (_load)
            return;

        ConfigBinding.SetLanguage(value);
    }

    partial void OnWindowTranTypeChanged(int value)
    {
        SaveWindowSetting();
    }

    partial void OnWarnColorChanged(Color value)
    {
        SetLogColor();
    }

    partial void OnErrorColorChanged(Color value)
    {
        SetLogColor();
    }

    partial void OnDebugColorChanged(Color value)
    {
        SetLogColor();
    }

    /// <summary>
    /// 打开运行路径
    /// </summary>
    [RelayCommand]
    public void OpenRunDir()
    {
        PathBinding.OpenPath(PathType.RunPath);
    }
    /// <summary>
    /// 重载颜色设置
    /// </summary>
    [RelayCommand]
    public void ColorReset()
    {
        _load = true;
        ConfigBinding.ResetColor();
        MainColor = Color.Parse(ThemeManager.MainColorStr);
        _load = false;
        Window.Notify(LangUtils.Get("Text.Reset"));
    }
    /// <summary>
    /// 设置背景图片大小
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SetPicSize()
    {
        var dialog = Window.ShowProgress(LangUtils.Get("SettingWindow.Tab2.Text70"));
        await ConfigBinding.SetBackLimit(EnablePicResize, PicResize);
        Window.CloseDialog(dialog);
        Window.Notify(LangUtils.Get("SettingWindow.Tab2.Text71"));
    }
    /// <summary>
    /// 设置背景图片透明
    /// </summary>
    [RelayCommand]
    public void SetPicTran()
    {
        ConfigBinding.SetBackTran(PicTran);
        Window.Notify(LangUtils.Get("SettingWindow.Tab2.Text71"));
    }
    /// <summary>
    /// 删除背景图
    /// </summary>
    [RelayCommand]
    public void DeletePic()
    {
        Pic = "";

        ConfigBinding.DeleteGuiImageConfig();
    }
    /// <summary>
    /// 选中背景图
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task OpenPic()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.Pic);
        if (file.Path != null)
        {
            Pic = file.Path;

            if (_load)
            {
                return;
            }

            await SetPic();
        }
    }
    /// <summary>
    /// 设置背景图
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SetPic()
    {
        if (_load)
        {
            return;
        }

        var dialog = Window.ShowProgress(LangUtils.Get("SettingWindow.Tab2.Text70"));
        await ConfigBinding.SetBackPic(EnableBG, Pic, PicEffect);
        Window.CloseDialog(dialog);

        Window.Notify(LangUtils.Get("SettingWindow.Tab2.Text71"));
    }
    /// <summary>
    /// 加载UI设置
    /// </summary>
    public void LoadUISetting()
    {
        _load = true;

        FontList.Clear();

        foreach (var item in FontManager.Current.SystemFonts)
        {
            FontList.Add(new()
            {
                FontName = item.Name,
                FontFamily = item
            });
        }

        var config = GuiConfigUtils.Config;
        if (config is { } con)
        {
            Pic = con.BackImage;
            EnableBG = con.EnableBG;
            PicEffect = con.BackEffect;
            PicTran = con.BackTran;
            RgbV1 = con.RGBS;
            RgbV2 = con.RGBV;
            PicResize = con.BackLimitValue;
            WindowTranType = con.WindowTranType;

            FontItem = FontList.FirstOrDefault(a => a.FontName == con.FontName);

            switch (con.ColorType)
            {
                case ColorType.Auto:
                    IsAutoColor = true;
                    break;
                case ColorType.Light:
                    IsLightColor = true;
                    break;
                case ColorType.Dark:
                    IsDarkColor = true;
                    break;
            }
            if (Color.TryParse(con.ColorMain, out var color))
            {
                MainColor = color;
            }

            EnableRGB = con.RGB;
            IsCutsomFont = !con.FontDefault;
            WindowMode = con.WindowMode;
            EnablePicResize = con.BackLimit;
            EnableWindowTran = con.WindowTran;

            AmTime = con.Style.AmTime;
            AmFade = con.Style.AmFade;
            EnableAm = con.Style.EnableAm;

            CardNews = con.Card.News;
            CardLast = con.Card.Last;
            CardOnline = con.Card.Online;
            CardBlock = con.Card.Block;

            HeadType = con.Head.Type;

            HeadX = con.Head.X;
            HeadY = con.Head.Y;

            if (Color.TryParse(con.LogColor.Warn, out color))
            {
                WarnColor = color;
            }
            if (Color.TryParse(con.LogColor.Error, out color))
            {
                ErrorColor = color;
            }
            if (Color.TryParse(con.LogColor.Debug, out color))
            {
                DebugColor = color;
            }
        }
        var config1 = GuiConfigUtils.Config;
        if (config1 is { } con1)
        {
            Language = con1.Language;
        }

        _load = false;
    }
    /// <summary>
    /// 主题色修改
    /// </summary>
    private void ColorChange()
    {
        if (_load)
            return;

        ConfigBinding.SetColor(MainColor.ToString());
    }
    /// <summary>
    /// 设置窗口设置
    /// </summary>
    private void SaveWindowSetting()
    {
        if (_load)
            return;

        ConfigBinding.SetWindowTran(EnableWindowTran, WindowTranType);
    }
    /// <summary>
    /// 设置RGB模式
    /// </summary>
    private void SetRgb()
    {
        if (_load)
            return;

        ConfigBinding.SetRgb(RgbV1, RgbV2);
    }
    /// <summary>
    /// 设置日志颜色
    /// </summary>
    private void SetLogColor()
    {
        if (_load)
            return;

        ConfigBinding.SetLogColor(WarnColor.ToString(), ErrorColor.ToString(), DebugColor.ToString());
    }
    /// <summary>
    /// 这是动画样式
    /// </summary>
    private void SetAm()
    {
        if (_load)
            return;

        ConfigBinding.SetStyle(AmTime, AmFade, EnableAm);
    }
    /// <summary>
    /// 设置卡片
    /// </summary>
    private void SetCard()
    {
        if (_load)
            return;

        ConfigBinding.SetCard(CardNews, CardLast, CardOnline, CardBlock);
    }
}
