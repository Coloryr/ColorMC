﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
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
    public ObservableCollection<FontDisplayModel> FontList { get; init; } = [];
    public string[] TranTypeList { get; init; } = LanguageBinding.GetWindowTranTypes();
    public string[] LanguageList { get; init; } = LanguageBinding.GetLanguages();
    public string[] PosList { get; init; } = LanguageBinding.GetPos();

    [ObservableProperty]
    private FontDisplayModel? _fontItem;

    [ObservableProperty]
    private Color _mainColor;

    [ObservableProperty]
    private bool _windowMode;
    [ObservableProperty]
    private bool _isCutsomFont;
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
    private bool _lowFps;

    [ObservableProperty]
    private bool _isHead1;
    [ObservableProperty]
    private bool _isHead2;
    [ObservableProperty]
    private bool _isHead3;
    [ObservableProperty]
    private int _headX;
    [ObservableProperty]
    private int _headY;

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
    private int _amTime;
    [ObservableProperty]
    private int _l2dPos;

    [ObservableProperty]
    private string? _pic;
    [ObservableProperty]
    private string? _live2DModel;

    [ObservableProperty]
    private string _live2DCoreState;

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

    private bool _load = true;

    partial void OnIsHead1Changed(bool value)
    {
        if (_load || !value)
            return;

        ConfigBinding.SetHeadType(HeadType.Head2D);
    }

    partial void OnIsHead2Changed(bool value)
    {
        if (_load || !value)
            return;

        ConfigBinding.SetHeadType(HeadType.Head3D_A);
    }

    partial void OnIsHead3Changed(bool value)
    {
        if (_load || !value)
            return;

        ConfigBinding.SetHeadType(HeadType.Head3D_B);
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

    [RelayCommand]
    public void OpenRunDir()
    {
        PathBinding.OpenPath(PathType.RunPath);
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
        MainColor = Color.Parse(ThemeManager.MainColorStr);
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
            EnableRGB = con.RGB;
            IsCutsomFont = !con.FontDefault;
            WindowMode = con.WindowMode;
            EnablePicResize = con.BackLimit;
            EnableWindowTran = con.WindowTran;

            AmTime = con.Style.AmTime;
            AmFade = con.Style.AmFade;

            Live2DModel = con.Live2D.Model;
            L2dHeight = con.Live2D.Height;
            L2dWidth = con.Live2D.Width;
            EnableLive2D = con.Live2D.Enable;
            L2dPos = con.Live2D.Pos;
            LowFps = con.Live2D.LowFps;

            switch (con.Head.Type)
            {
                case HeadType.Head2D:
                    IsHead1 = true;
                    IsHead2 = false;
                    IsHead3 = false;
                    break;
                case HeadType.Head3D_A:
                    IsHead1 = false;
                    IsHead2 = true;
                    IsHead3 = false;
                    break;
                case HeadType.Head3D_B:
                    IsHead1 = false;
                    IsHead2 = false;
                    IsHead3 = true;
                    break;
            };

            HeadX = con.Head.X;
            HeadY = con.Head.Y;
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

    private void ColorChange()
    {
        if (_load)
            return;

        ConfigBinding.SetColor(MainColor.ToString());
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
