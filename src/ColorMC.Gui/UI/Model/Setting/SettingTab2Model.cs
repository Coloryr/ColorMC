using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Live2DCSharpSDK.Framework.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab2Model : BaseModel
{
    public ObservableCollection<FontDisplay> FontList { get; init; } = new();
    public List<string> TranTypeList => LanguageUtils.GetWindowTranTypes();
    public List<string> LanguageList => LanguageUtils.GetLanguages();

    [ObservableProperty]
    private FontDisplay? _fontItem;

    [ObservableProperty]
    private Color _mainColor;

    [ObservableProperty]
    private Color _lightBackColor;
    [ObservableProperty]
    private Color _lightTranColor;
    [ObservableProperty]
    private Color _lightFont1Color;
    [ObservableProperty]
    private Color _lightFont2Color;

    [ObservableProperty]
    private Color _darkBackColor;
    [ObservableProperty]
    private Color _darkTranColor;
    [ObservableProperty]
    private Color _darkFont1Color;
    [ObservableProperty]
    private Color _darkFont2Color;

    [ObservableProperty]
    private bool _windowMode;
    [ObservableProperty]
    private bool _isDefaultFont;
    [ObservableProperty]
    private bool _enableFontList;
    [ObservableProperty]
    private bool _enablePicResize;
    [ObservableProperty]
    private bool _isAutoColor;
    [ObservableProperty]
    private bool _isLightColor;
    [ObservableProperty]
    private bool _isDarkColor;
    [ObservableProperty]
    private bool _enableRGB;
    [ObservableProperty]
    private bool _enableWindowTran;
    [ObservableProperty]
    private bool _enableWindowMode = true;
    [ObservableProperty]
    private bool _coreInstall;
    [ObservableProperty]
    private bool _mainWindowStateSave;
    [ObservableProperty]
    private bool _mainWindowMirror;
    [ObservableProperty]
    private bool _amFade;

    [ObservableProperty]
    private int _language;
    [ObservableProperty]
    private int _picEffect;
    [ObservableProperty]
    private int _picTran;
    [ObservableProperty]
    private int _picResize;
    [ObservableProperty]
    private int _windowTranType;
    [ObservableProperty]
    private int _rgbV1;
    [ObservableProperty]
    private int _rgbV2;
    [ObservableProperty]
    private int _width;
    [ObservableProperty]
    private int _height;
    [ObservableProperty]
    private int _buttonCornerRadius;
    [ObservableProperty]
    private int _amTime;

    [ObservableProperty]
    private string? _pic;
    [ObservableProperty]
    private string? _live2DModel;

    [ObservableProperty]
    private string _live2DCoreState;

    private bool _load = false;

    public SettingTab2Model(IUserControl con) : base(con)
    {
        if (SystemInfo.Os == OsType.Linux)
        {
            _enableWindowMode = false;
        }
    }

    partial void OnAmFadeChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetStyle1(AmTime, AmFade);
    }

    partial void OnAmTimeChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetStyle1(AmTime, AmFade);
    }

    partial void OnButtonCornerRadiusChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetStyle(ButtonCornerRadius);
    }

    partial void OnMainWindowStateSaveChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetWindowStateSave(MainWindowStateSave);
    }

    partial void OnMainWindowMirrorChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetMainWindow(MainWindowMirror);
    }

    partial void OnWidthChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetLive2DSize(Width, Height);
    }

    partial void OnHeightChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetLive2DSize(Width, Height);
    }

    partial void OnLightFont2ColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnLightFont1ColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnLightTranColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnLightBackColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnDarkFont2ColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnDarkFont1ColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnDarkTranColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnDarkBackColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnMainColorChanged(Color value)
    {
        ColorChange();
    }

    partial void OnFontItemChanged(FontDisplay? value)
    {
        if (_load || value == null)
            return;

        OnPropertyChanged("Hide");

        ConfigBinding.SetFont(value.FontName, IsDefaultFont);
    }

    partial void OnEnableWindowTranChanged(bool value)
    {
        Save1();
    }

    partial void OnEnableRGBChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetRgb(value);
    }

    partial void OnWindowModeChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetWindowMode(value);
    }

    partial void OnIsAutoColorChanged(bool value)
    {
        if (_load)
            return;

        if (value)
        {
            ConfigBinding.SetColorType(ColorType.Auto);
        }
    }

    partial void OnIsLightColorChanged(bool value)
    {
        if (_load)
            return;

        if (value)
        {
            ConfigBinding.SetColorType(ColorType.Light);
        }
    }

    partial void OnIsDarkColorChanged(bool value)
    {
        if (_load)
            return;

        if (value)
        {
            ConfigBinding.SetColorType(ColorType.Dark);
        }
    }

    async partial void OnEnablePicResizeChanged(bool value)
    {
        if (_load)
            return;

        if (value)
        {
            Progress(App.GetLanguage("SettingWindow.Tab2.Info2"));
            await ConfigBinding.SetBackLimit(value, PicResize);
            ProgressClose();
        }
    }

    partial void OnIsDefaultFontChanged(bool value)
    {
        if (value == true)
        {
            EnableFontList = false;
        }
        else
        {
            EnableFontList = true;
        }

        if (_load)
            return;

        ConfigBinding.SetFont(FontItem?.FontName, value);
    }

    partial void OnLanguageChanged(int value)
    {
        if (_load)
            return;

        var type = (LanguageType)value;
        Progress(App.GetLanguage("SettingWindow.Tab2.Info1"));
        ConfigBinding.SetLanguage(type);
        ProgressClose();
    }

    partial void OnWindowTranTypeChanged(int value)
    {
        Save1();
    }

    [RelayCommand]
    public void OpenRunDir()
    {
        BaseBinding.OpenRunDir();
    }

    [RelayCommand]
    public void DownloadCore()
    {
        BaseBinding.OpenLive2DCore();
    }

    [RelayCommand]
    public void SetRgb()
    {
        ConfigBinding.SetRgb(RgbV1, RgbV2);
    }

    [RelayCommand]
    public void ColorReset()
    {
        _load = true;
        ConfigBinding.ResetColor();
        MainColor = ColorSel.MainColor.ToColor();
        LightBackColor = Color.Parse(ColorSel.BackLigthColorStr);
        LightTranColor = Color.Parse(ColorSel.Back1LigthColorStr);
        LightFont1Color = Color.Parse(ColorSel.ButtonLightFontStr);
        LightFont2Color = Color.Parse(ColorSel.FontLigthColorStr);
        DarkBackColor = Color.Parse(ColorSel.BackDarkColorStr);
        DarkTranColor = Color.Parse(ColorSel.Back1DarkColorStr);
        DarkFont1Color = Color.Parse(ColorSel.ButtonDarkFontStr);
        DarkFont2Color = Color.Parse(ColorSel.FontDarkColorStr);
        _load = false;
        Notify(App.GetLanguage("SettingWindow.Tab2.Info4"));
    }

    [RelayCommand]
    public async Task SetPicSize()
    {
        Progress(App.GetLanguage("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackLimit(EnablePicResize, PicResize);
        ProgressClose();

        Notify(App.GetLanguage("Gui.Info3"));
    }

    [RelayCommand]
    public void SetPicTran()
    {
        ConfigBinding.SetBackTran(PicTran);
        Notify(App.GetLanguage("Gui.Info3"));
    }

    [RelayCommand]
    public void DeletePic()
    {
        Pic = "";

        ConfigBinding.DeleteGuiImageConfig();
    }

    [RelayCommand]
    public async Task OpenPic()
    {
        var file = await BaseBinding.OpFile(Window, FileType.Pic);

        if (file != null)
        {
            Pic = file;

            if (_load)
                return;

            await SetPic();
        }
    }

    [RelayCommand]
    public async Task SetPic()
    {
        if (_load)
            return;

        if (string.IsNullOrWhiteSpace(Pic))
        {
            Show(App.GetLanguage("SettingWindow.Tab2.Error1"));
            return;
        }
        Progress(App.GetLanguage("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackPic(Pic, PicEffect);
        ProgressClose();

        Notify(App.GetLanguage("Gui.Info3"));
    }

    [RelayCommand]
    public void DeleteLive2D()
    {
        Live2DModel = "";

        ConfigBinding.DeleteLive2D();
    }

    [RelayCommand]
    public async Task OpenLive2D()
    {
        var file = await BaseBinding.OpFile(Window, FileType.Live2D);

        if (file != null)
        {
            Live2DModel = file;

            if (_load)
                return;

            SetLive2D();
        }
    }

    [RelayCommand]
    public void SetLive2D()
    {
        if (_load)
            return;

        if (string.IsNullOrWhiteSpace(Live2DModel))
        {
            Show(App.GetLanguage("SettingWindow.Tab2.Error3"));
            return;
        }
        Progress(App.GetLanguage("SettingWindow.Tab2.Info2"));
        ConfigBinding.SetLive2D(Live2DModel);
        ProgressClose();

        Notify(App.GetLanguage("Gui.Info3"));
    }

    public void Load()
    {
        _load = true;

        FontList.Clear();
        BaseBinding.GetFontList().ForEach(item =>
        {
            FontList.Add(new()
            {
                FontName = item.Name,
                FontFamily = item
            });
        });

        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 != null)
        {
            Pic = config.Item2.BackImage;
            PicEffect = config.Item2.BackEffect;
            PicTran = config.Item2.BackTran;
            RgbV1 = config.Item2.RGBS;
            RgbV2 = config.Item2.RGBV;
            PicResize = config.Item2.BackLimitValue;
            WindowTranType = config.Item2.WindowTranType;

            FontItem = FontList.FirstOrDefault(a => a.FontName == config.Item2.FontName);

            switch (config.Item2.ColorType)
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
            MainColor = Color.Parse(config.Item2.ColorMain);
            LightBackColor = Color.Parse(config.Item2.ColorLight.ColorBack);
            LightTranColor = Color.Parse(config.Item2.ColorLight.ColorTranBack);
            LightFont1Color = Color.Parse(config.Item2.ColorLight.ColorFont1);
            LightFont2Color = Color.Parse(config.Item2.ColorLight.ColorFont2);
            DarkBackColor = Color.Parse(config.Item2.ColorDark.ColorBack);
            DarkTranColor = Color.Parse(config.Item2.ColorDark.ColorTranBack);
            DarkFont1Color = Color.Parse(config.Item2.ColorDark.ColorFont1);
            DarkFont2Color = Color.Parse(config.Item2.ColorDark.ColorFont2);
            EnableRGB = config.Item2.RGB;
            IsDefaultFont = config.Item2.FontDefault;
            EnableFontList = !IsDefaultFont;
            WindowMode = config.Item2.WindowMode;
            EnablePicResize = config.Item2.BackLimit;
            EnableWindowTran = config.Item2.WindowTran;

            MainWindowMirror = config.Item2.Gui.WindowMirror;
            MainWindowStateSave = config.Item2.Gui.WindowStateSave;

            ButtonCornerRadius = config.Item2.Style.ButtonCornerRadius;
            AmTime = config.Item2.Style.AmTime;
            AmFade = config.Item2.Style.AmFade;

            Live2DModel = config.Item2.Live2D.Model;
            Height = config.Item2.Live2D.Height;
            Width = config.Item2.Live2D.Width;
        }
        if (config.Item1 != null)
        {
            Language = (int)config.Item1.Language;
        }

        try
        {
            var version = CubismCore.Version();

            uint major = (version & 0xFF000000) >> 24;
            uint minor = (version & 0x00FF0000) >> 16;
            uint patch = version & 0x0000FFFF;
            uint vesionNumber = version;

            Live2DCoreState = $"version: {major:##}.{minor:#}.{patch:####} ({vesionNumber})";
            CoreInstall = true;
        }
        catch
        {
            Live2DCoreState = App.GetLanguage("SettingWindow.Tab2.Error2");
            CoreInstall = false;
        }

        _load = false;
    }

    private void ColorChange()
    {
        if (_load)
            return;

        ConfigBinding.SetColor(MainColor.ToString(),
            LightBackColor.ToString(), LightTranColor.ToString(),
            LightFont1Color.ToString(), LightFont2Color.ToString(),
            DarkBackColor.ToString(), DarkTranColor.ToString(),
            DarkFont1Color.ToString(), DarkFont2Color.ToString());
    }

    private void Save1()
    {
        if (_load)
            return;

        Progress(App.GetLanguage("SettingWindow.Tab2.Info5"));
        ConfigBinding.SetWindowTran(EnableWindowTran, WindowTranType);
        ProgressClose();
    }
}
