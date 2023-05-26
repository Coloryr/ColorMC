using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.Setting;

public record FontDisplay
{
    public string FontName { get; init; }
    public FontFamily FontFamily { get; init; }

    public override string ToString()
    {
        return FontName;
    }
}

public partial class SettingTab2Model : ObservableObject
{
    private readonly IUserControl Con;

    public ObservableCollection<FontDisplay> FontList { get; init; } = new();
    public List<string> TranTypeList => BaseBinding.GetWindowTranTypes();
    public List<string> LanguageList => BaseBinding.GetLanguages();

    [ObservableProperty]
    private FontDisplay? fontItem;

    [ObservableProperty]
    private Color mainColor;

    [ObservableProperty]
    private Color lightBackColor;
    [ObservableProperty]
    private Color lightTranColor;
    [ObservableProperty]
    private Color lightFont1Color;
    [ObservableProperty]
    private Color lightFont2Color;

    [ObservableProperty]
    private Color darkBackColor;
    [ObservableProperty]
    private Color darkTranColor;
    [ObservableProperty]
    private Color darkFont1Color;
    [ObservableProperty]
    private Color darkFont2Color;

    [ObservableProperty]
    private bool windowMode;
    [ObservableProperty]
    private bool isDefaultFont;
    [ObservableProperty]
    private bool enableFontList;
    [ObservableProperty]
    private bool enablePicResize;
    [ObservableProperty]
    private bool isAutoColor;
    [ObservableProperty]
    private bool isLightColor;
    [ObservableProperty]
    private bool isDarkColor;
    [ObservableProperty]
    private bool enableRGB;
    [ObservableProperty]
    private bool enableWindowTran;
    [ObservableProperty]
    private bool enableWindowMode = true;

    [ObservableProperty]
    private int language;
    [ObservableProperty]
    private int picEffect;
    [ObservableProperty]
    private int picTran;
    [ObservableProperty]
    private int picResize;
    [ObservableProperty]
    private int windowTranType;
    [ObservableProperty]
    private int rgbV1;
    [ObservableProperty]
    private int rgbV2;

    [ObservableProperty]
    private string? pic;

    private bool load = false;

    public SettingTab2Model(IUserControl con)
    {
        Con = con;

        if (SystemInfo.Os == OsType.Linux)
        {
            enableWindowMode = false;
        }
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
        if (load || value == null)
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
        ConfigBinding.SetRgb(value);
    }

    partial void OnWindowModeChanged(bool value)
    {
        ConfigBinding.SetWindowMode(value);
    }

    partial void OnIsAutoColorChanged(bool value)
    {
        if (load)
            return;

        if (value)
        {
            ConfigBinding.SetColorType(ColorType.Auto);
        }
    }

    partial void OnIsLightColorChanged(bool value)
    {
        if (load)
            return;

        if (value)
        {
            ConfigBinding.SetColorType(ColorType.Light);
        }
    }

    partial void OnIsDarkColorChanged(bool value)
    {
        if (load)
            return;

        if (value)
        {
            ConfigBinding.SetColorType(ColorType.Dark);
        }
    }

    async partial void OnEnablePicResizeChanged(bool value)
    {
        if (value)
        {
            var window = Con.Window;
            window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info2"));
            await ConfigBinding.SetBackLimit(value, PicResize);
            window.ProgressInfo.Close();
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

        ConfigBinding.SetFont(FontItem?.FontName, value);
    }

    partial void OnLanguageChanged(int value)
    {
        if (load)
            return;

        var window = Con.Window;
        var type = (LanguageType)value;
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info1"));
        ConfigBinding.SetLanguage(type);
        window.ProgressInfo.Close();
    }

    partial void OnWindowTranTypeChanged(int value)
    {
        Save1();
    }

    [RelayCommand]
    public void SetRgb()
    {
        ConfigBinding.SetRgb(RgbV1, RgbV2);
    }

    [RelayCommand]
    public void ColorReset()
    {
        var window = Con.Window;
        load = true;
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
        load = false;
        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info4"));
    }

    [RelayCommand]
    public async void SetPicSize()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackLimit(EnablePicResize, PicResize);
        window.ProgressInfo.Close();

        window.NotifyInfo.Show(App.GetLanguage("Gui.Info3"));
    }

    [RelayCommand]
    public void DeletePic()
    {
        Pic = "";

        ConfigBinding.DeleteGuiImageConfig();
    }

    [RelayCommand]
    public async void OpenPic()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, FileType.Pic);

        if (file != null)
        {
            Pic = file;

            SetPic();
        }
    }

    [RelayCommand]
    public void SetPicTran()
    {
        var window = Con.Window;
        ConfigBinding.SetBackTran(PicTran);
        window.NotifyInfo.Show(App.GetLanguage("Gui.Info3"));
    }

    [RelayCommand]
    public async void SetPic()
    {
        var window = Con.Window;
        if (string.IsNullOrWhiteSpace(Pic))
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab2.Error1"));
            return;
        }
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info2"));
        await ConfigBinding.SetBackPic(Pic, PicEffect);
        window.ProgressInfo.Close();

        window.NotifyInfo.Show(App.GetLanguage("Gui.Info3"));
    }

    public void Load()
    {
        load = true;

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
            WindowMode = config.Item2.WindowMode;
            EnablePicResize = config.Item2.BackLimit;
            EnableWindowTran = config.Item2.WindowTran;
        }
        if (config.Item1 != null)
        {
            Language = (int)config.Item1.Language;
        }

        load = false;
    }

    private void ColorChange()
    {
        if (load)
            return;

        ConfigBinding.SetColor(MainColor.ToString(),
            LightBackColor.ToString(), LightTranColor.ToString(),
            LightFont1Color.ToString(), LightFont2Color.ToString(),
            DarkBackColor.ToString(), DarkTranColor.ToString(),
            DarkFont1Color.ToString(), DarkFont2Color.ToString());
    }

    private void Save1()
    {
        if (load)
            return;

        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab2.Info5"));
        ConfigBinding.SetWindowTran(EnableWindowTran, WindowTranType);
        window.ProgressInfo.Close();
    }
}
