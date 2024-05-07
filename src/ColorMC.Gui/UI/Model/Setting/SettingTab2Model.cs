using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Live2DCSharpSDK.Framework.Core;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    public ObservableCollection<FontDisplayObj> FontList { get; init; } = [];
    public string[] TranTypeList { get; init; } = LanguageBinding.GetWindowTranTypes();
    public string[] LanguageList { get; init; } = LanguageBinding.GetLanguages();
    public string[] PosList { get; init; } = LanguageBinding.GetPos();

    [ObservableProperty]
    private FontDisplayObj? _fontItem;

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
    private bool _amFade;
    [ObservableProperty]
    private bool _enableBG;
    [ObservableProperty]
    private bool _enableLive2D;
    [ObservableProperty]
    private bool _enablePicRadius;
    [ObservableProperty]
    private bool _enableBorderRadius;
    [ObservableProperty]
    private bool _lowFps;

    [ObservableProperty]
    private LanguageType _language;
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
    private int _l2dWidth;
    [ObservableProperty]
    private int _l2dHeight;
    [ObservableProperty]
    private int _buttonCornerRadius;
    [ObservableProperty]
    private int _amTime;
    [ObservableProperty]
    private int _l2dPos;

    [ObservableProperty]
    private string? _pic;
    [ObservableProperty]
    private string? _live2DModel;

    [ObservableProperty]
    private string _live2DCoreState;

    private bool _load = true;


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

    partial void OnEnableBorderRadiusChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetRadiusEnable(EnablePicRadius, EnableBorderRadius);
    }

    partial void OnEnablePicRadiusChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetRadiusEnable(EnablePicRadius, EnableBorderRadius);
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

    partial void OnAmFadeChanged(bool value)
    {
        if (_load)
            return;

        ConfigBinding.SetStyle(AmTime, AmFade);
    }

    partial void OnAmTimeChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetStyle(AmTime, AmFade);
    }

    partial void OnButtonCornerRadiusChanged(int value)
    {
        if (_load)
            return;

        ConfigBinding.SetStyle(ButtonCornerRadius);
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

    partial void OnFontItemChanged(FontDisplayObj? value)
    {
        if (_load || value == null)
            return;

        OnPropertyChanged("Hide");

        ConfigBinding.SetFont(value.FontName, IsDefaultFont);
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
        if (_load)
            return;

        SetRgb();
    }

    partial void OnRgbV2Changed(int value)
    {
        if (_load)
            return;

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
            Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
            await ConfigBinding.SetBackLimit(value, PicResize);
            Model.ProgressClose();
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

    [RelayCommand]
    public async Task InstallCore()
    {
        var file = await PathBinding.SelectFile(FileType.Live2DCore);
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
                App.Reboot();
            }
        }
    }

    [RelayCommand]
    public void OpenRunDir()
    {
        PathBinding.OpPath(PathType.RunPath);
    }

    [RelayCommand]
    public void DownloadCore()
    {
        WebBinding.OpenWeb(WebType.Live2DCore);
    }

    [RelayCommand]
    public void ColorReset()
    {
        _load = true;
        ConfigBinding.ResetColor();
        MainColor = Color.Parse(ColorSel.MainColorStr);
        LightBackColor = Color.Parse(ColorSel.BackLigthColorStr);
        LightTranColor = Color.Parse(ColorSel.Back1LigthColorStr);
        LightFont1Color = Color.Parse(ColorSel.ButtonLightFontStr);
        LightFont2Color = Color.Parse(ColorSel.FontLigthColorStr);
        DarkBackColor = Color.Parse(ColorSel.BackDarkColorStr);
        DarkTranColor = Color.Parse(ColorSel.Back1DarkColorStr);
        DarkFont1Color = Color.Parse(ColorSel.ButtonDarkFontStr);
        DarkFont2Color = Color.Parse(ColorSel.FontDarkColorStr);
        _load = false;
        Model.Notify(App.Lang("SettingWindow.Tab2.Info4"));
    }

    [RelayCommand]
    public async Task SetPicSize()
    {
        Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackLimit(EnablePicResize, PicResize);
        Model.ProgressClose();

        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
    }

    [RelayCommand]
    public void SetPicTran()
    {
        ConfigBinding.SetBackTran(PicTran);
        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
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
        var file = await PathBinding.SelectFile(FileType.Pic);
        if (file.Item1 != null)
        {
            Pic = file.Item1;

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

        Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
        if (SystemInfo.Os == OsType.Android)
        {
            await PathBinding.CopyBG(Pic!);
        }
        await ConfigBinding.SetBackPic(EnableBG, Pic, PicEffect);
        Model.ProgressClose();

        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
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
        var file = await PathBinding.SelectFile(FileType.Live2D);
        if (file.Item1 != null)
        {
            Live2DModel = file.Item1;

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
            Model.Show(App.Lang("SettingWindow.Tab2.Error3"));
            return;
        }
        Model.Progress(App.Lang("SettingWindow.Tab2.Info2"));
        ConfigBinding.SetLive2D(Live2DModel);
        Model.ProgressClose();

        Model.Notify(App.Lang("SettingWindow.Tab2.Info12"));
    }

    public void LoadUISetting()
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
        if (config.Item2 is { } con)
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
            MainColor = Color.Parse(con.ColorMain);
            LightBackColor = Color.Parse(con.ColorLight.ColorBack);
            LightTranColor = Color.Parse(con.ColorLight.ColorTranBack);
            LightFont1Color = Color.Parse(con.ColorLight.ColorFont1);
            LightFont2Color = Color.Parse(con.ColorLight.ColorFont2);
            DarkBackColor = Color.Parse(con.ColorDark.ColorBack);
            DarkTranColor = Color.Parse(con.ColorDark.ColorTranBack);
            DarkFont1Color = Color.Parse(con.ColorDark.ColorFont1);
            DarkFont2Color = Color.Parse(con.ColorDark.ColorFont2);
            EnableRGB = con.RGB;
            IsDefaultFont = con.FontDefault;
            EnableFontList = !IsDefaultFont;
            WindowMode = con.WindowMode;
            EnablePicResize = con.BackLimit;
            EnableWindowTran = con.WindowTran;

            ButtonCornerRadius = con.Style.ButtonCornerRadius;
            AmTime = con.Style.AmTime;
            AmFade = con.Style.AmFade;

            Live2DModel = con.Live2D.Model;
            L2dHeight = con.Live2D.Height;
            L2dWidth = con.Live2D.Width;
            EnableLive2D = con.Live2D.Enable;
            L2dPos = con.Live2D.Pos;
            LowFps = con.Live2D.LowFps;
        }
        if (config.Item1 is { } con1)
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

    private void SaveWindowSetting()
    {
        if (_load)
            return;

        Model.Progress(App.Lang("SettingWindow.Tab2.Info5"));
        ConfigBinding.SetWindowTran(EnableWindowTran, WindowTranType);
        Model.ProgressClose();
    }

    private void SetRgb()
    {
        ConfigBinding.SetRgb(RgbV1, RgbV2);
    }
}
