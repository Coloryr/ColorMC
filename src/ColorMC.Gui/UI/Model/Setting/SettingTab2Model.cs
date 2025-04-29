using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Live2DCSharpSDK.Framework.Core;

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
    public string[] TranTypeList { get; init; } = LanguageBinding.GetWindowTranTypes();
    /// <summary>
    /// 语言列表
    /// </summary>
    public string[] LanguageList { get; init; } = LanguageBinding.GetLanguages();
    /// <summary>
    /// 位置列表
    /// </summary>
    public string[] PosList { get; init; } = LanguageBinding.GetPos();

    /// <summary>
    /// 字体
    /// </summary>
    [ObservableProperty]
    private FontDisplayModel? _fontItem;

    /// <summary>
    /// 主要颜色
    /// </summary>
    [ObservableProperty]
    private Color _mainColor;
    /// <summary>
    /// 警告颜色
    /// </summary>
    [ObservableProperty]
    private Color _warnColor;
    /// <summary>
    /// 错误颜色
    /// </summary>
    [ObservableProperty]
    private Color _errorColor;
    /// <summary>
    /// 调试颜色
    /// </summary>
    [ObservableProperty]
    private Color _debugColor;

    /// <summary>
    /// 是否启用单窗口模式
    /// </summary>
    [ObservableProperty]
    private bool _windowMode;
    /// <summary>
    /// 是否使用自定义字体
    /// </summary>
    [ObservableProperty]
    private bool _isCutsomFont;
    /// <summary>
    /// 是否启用背景图分辨率缩放
    /// </summary>
    [ObservableProperty]
    private bool _enablePicResize;
    /// <summary>
    /// 是否启动自动主题
    /// </summary>
    [ObservableProperty]
    private bool _isAutoColor;
    /// <summary>
    /// 是否启用亮主题
    /// </summary>
    [ObservableProperty]
    private bool _isLightColor;
    /// <summary>
    /// 是否启用暗主题
    /// </summary>
    [ObservableProperty]
    private bool _isDarkColor;
    /// <summary>
    /// 是否启用RGB模式
    /// </summary>
    [ObservableProperty]
    private bool _enableRGB;
    /// <summary>
    /// 是否启用窗口透明
    /// </summary>
    [ObservableProperty]
    private bool _enableWindowTran;
    /// <summary>
    /// Live2d核心是否安装
    /// </summary>
    [ObservableProperty]
    private bool _coreInstall;
    /// <summary>
    /// 是否启用动画虚化
    /// </summary>
    [ObservableProperty]
    private bool _amFade;
    /// <summary>
    /// 是否启用自定义背景图
    /// </summary>
    [ObservableProperty]
    private bool _enableBG;
    /// <summary>
    /// 是否启用Live2d
    /// </summary>
    [ObservableProperty]
    private bool _enableLive2D;
    /// <summary>
    /// 是否降低FPS
    /// </summary>
    [ObservableProperty]
    private bool _lowFps;
    /// <summary>
    /// 是否启用动画
    /// </summary>
    [ObservableProperty]
    private bool _enableAm;
    /// <summary>
    /// 是否启用Minecraft news
    /// </summary>
    [ObservableProperty]
    private bool _cardNews;
    /// <summary>
    /// 是否启用上次启动
    /// </summary>
    [ObservableProperty]
    private bool _cardLast;
    /// <summary>
    /// 是否启用在线联机
    /// </summary>
    [ObservableProperty]
    private bool _cardOnline;
    /// <summary>
    /// 简易主界面
    /// </summary>
    [ObservableProperty]
    private bool _simple;

    /// <summary>
    /// 头像旋转
    /// </summary>
    [ObservableProperty]
    private int _headX;
    /// <summary>
    /// 头像旋转
    /// </summary>
    [ObservableProperty]
    private int _headY;
    /// <summary>
    /// 选中的语言
    /// </summary>
    [ObservableProperty]
    private LanguageType _language;
    /// <summary>
    /// 背景图虚化
    /// </summary>
    [ObservableProperty]
    private int _picEffect;
    /// <summary>
    /// 背景图透明
    /// </summary>
    [ObservableProperty]
    private int _picTran;
    /// <summary>
    /// 背景图缩小分辨率
    /// </summary>
    [ObservableProperty]
    private int _picResize;
    /// <summary>
    /// 窗口透明模式
    /// </summary>
    [ObservableProperty]
    private int _windowTranType;
    /// <summary>
    /// RGB
    /// </summary>
    [ObservableProperty]
    private int _rgbV1;
    /// <summary>
    /// RGB
    /// </summary>
    [ObservableProperty]
    private int _rgbV2;
    /// <summary>
    /// Live2d宽度
    /// </summary>
    [ObservableProperty]
    private int _l2dWidth;
    /// <summary>
    /// Live2d高度
    /// </summary>
    [ObservableProperty]
    private int _l2dHeight;
    /// <summary>
    /// 动画时间
    /// </summary>
    [ObservableProperty]
    private int _amTime;
    /// <summary>
    /// Live2d位置
    /// </summary>
    [ObservableProperty]
    private int _l2dPos;

    /// <summary>
    /// 背景图位置
    /// </summary>
    [ObservableProperty]
    private string? _pic;
    /// <summary>
    /// Live2d模型位置
    /// </summary>
    [ObservableProperty]
    private string? _live2DModel;
    /// <summary>
    /// Live2d Core状态
    /// </summary>
    [ObservableProperty]
    private string _live2DCoreState;

    /// <summary>
    /// 头像模式
    /// </summary>
    [ObservableProperty]
    private HeadType _headType;

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
                return $"/Resource/Icon/Setting/svg{28 + random.Next(6)}.svg";
            }

            return "/Resource/Icon/Setting/svg27.svg";
        }
    }

    //配置修改
    partial void OnSimpleChanged(bool value)
    {
        if (_load)
        {
            return;
        }

        ConfigBinding.SetWindowSimple(value);
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

    partial void OnLowFpsChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetLive2DMode(value);
    }

    partial void OnL2dPosChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetLive2DSize(L2dWidth, L2dHeight, L2dPos);
    }

    partial void OnEnableLive2DChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetLive2D(value);
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

    partial void OnL2dWidthChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetLive2DSize(L2dWidth, L2dHeight, L2dPos);
    }

    partial void OnL2dHeightChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetLive2DSize(L2dWidth, L2dHeight, L2dPos);
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
            Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
            await ConfigBinding.SetBackLimit(value, PicResize);
            Model.ProgressClose();
        }
    }

    partial void OnLanguageChanged(LanguageType value)
    {
        if (_load)
            return;

        Model.Progress(App.Lang("SettingWindow.Tab2.Info1"));
        ConfigBinding.SetLanguage(value);
        Model.ProgressClose();
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
    /// 导入Live2d core
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task InstallCore()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Live2DCore);
        if (file.Item1 != null)
        {
            Model.Progress(App.Lang("SettingWindow.Tab2.Info11"));
            var res = await BaseBinding.SetLive2DCore(file.Item1);
            Model.ProgressClose();
            if (!res)
            {
                Model.Show(App.Lang("SettingWindow.Tab2.Error4"));
            }
            else
            {
                ColorMCGui.Reboot();
            }
        }
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
    /// 打开下载live2d core
    /// </summary>
    [RelayCommand]
    public void DownloadCore()
    {
        WebBinding.OpenWeb(WebType.Live2DCore);
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
        Model.Notify(App.Lang("SettingWindow.Tab2.Info4"));
    }
    /// <summary>
    /// 设置背景图片大小
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SetPicSize()
    {
        Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackLimit(EnablePicResize, PicResize);
        Model.ProgressClose();
        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
    }
    /// <summary>
    /// 设置背景图片透明
    /// </summary>
    [RelayCommand]
    public void SetPicTran()
    {
        ConfigBinding.SetBackTran(PicTran);
        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
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
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Pic);
        if (file.Item1 != null)
        {
            Pic = file.Item1;

            if (_load)
                return;

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

        Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackPic(EnableBG, Pic, PicEffect);
        Model.ProgressClose();

        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
    }
    /// <summary>
    /// 删除Live2d模型
    /// </summary>
    [RelayCommand]
    public void DeleteLive2D()
    {
        Live2DModel = "";

        ConfigBinding.DeleteLive2D();
    }
    /// <summary>
    /// 选中Live2d模型
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task OpenLive2D()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Live2D);
        if (file.Item1 != null)
        {
            Live2DModel = file.Item1;

            if (_load)
                return;

            SetLive2D();
        }
    }
    /// <summary>
    /// 设置Live2d模型
    /// </summary>
    [RelayCommand]
    public void SetLive2D()
    {
        if (_load)
            return;

        if (string.IsNullOrWhiteSpace(Live2DModel))
        {
            Model.Show(App.Lang("SettingWindow.Tab2.Error3"));
            return;
        }
        Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
        ConfigBinding.SetLive2D(Live2DModel);
        Model.ProgressClose();

        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
    }
    /// <summary>
    /// 加载UI设置
    /// </summary>
    public void LoadUISetting()
    {
        _load = true;

        FontList.Clear();

        foreach (var item in BaseBinding.GetFontList())
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
            Simple = con.Simple;

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

            Live2DModel = con.Live2D.Model;
            L2dHeight = con.Live2D.Height;
            L2dWidth = con.Live2D.Width;
            EnableLive2D = con.Live2D.Enable;
            L2dPos = con.Live2D.Pos;
            LowFps = con.Live2D.LowFps;
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
        var config1 = ConfigUtils.Config;
        if (config1 is { } con1)
        {
            Language = con1.Language;
        }

        try
        {
            var version = CubismCore.Version();

            uint major = (version & 0xFF000000) >> 24;
            uint minor = (version & 0x00FF0000) >> 16;
            uint patch = version & 0x0000FFFF;
            uint vesionNumber = version;

            Live2DCoreState = $"Version: {major:0}.{minor:0}.{patch:0000} ({vesionNumber})";
            CoreInstall = true;
        }
        catch
        {
            Live2DCoreState = App.Lang("SettingWindow.Tab2.Error2");
            CoreInstall = false;
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

        Model.Progress(App.Lang("SettingWindow.Tab2.Info5"));
        ConfigBinding.SetWindowTran(EnableWindowTran, WindowTranType);
        Model.ProgressClose();
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

        ConfigBinding.SetCard(CardNews, CardLast, CardOnline);
    }
}
