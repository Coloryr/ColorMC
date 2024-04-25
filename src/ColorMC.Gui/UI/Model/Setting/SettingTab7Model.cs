﻿using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    public Bitmap Bitmap => App.GameIcon;
    public string Version => ColorMCCore.Version;
    public string RunType => App.Lang(ColorMCGui.IsAot ? "SettingWindow.Tab7.Info1" : "SettingWindow.Tab7.Info2");

    [RelayCommand]
    public void OpenUrl1(object urls)
    {
        WebBinding.OpenWeb(WebType.Web);
    }

    [RelayCommand]
    public void OpenUrl2()
    {
        WebBinding.OpenWeb(WebType.Minecraft);
    }

    [RelayCommand]
    public void OpenUrl3()
    {
        WebBinding.OpenWeb(WebType.Sponsor);
    }

    [RelayCommand]
    public void OpenUrl4()
    {
        WebBinding.OpenWeb(WebType.Github);
    }

    [RelayCommand]
    public async Task OpenUrl5()
    {
        var res = await Model.ShowWait(App.Lang("SettingWindow.Tab7.Info3"));
        WebBinding.OpenWeb(res ? WebType.Guide1 : WebType.Guide);
    }

    [RelayCommand]
    public void OpenUrl6()
    {
        WebBinding.OpenWeb(WebType.Mcmod);
    }

    [RelayCommand]
    public void OpenUrl7()
    {
        WebBinding.OpenWeb(WebType.Apache2_0);
    }

    [RelayCommand]
    public void OpenUrl8()
    {
        WebBinding.OpenWeb(WebType.MIT);
    }

    [RelayCommand]
    public void OpenUrl9()
    {
        WebBinding.OpenWeb(WebType.MiSans);
    }

    [RelayCommand]
    public void OpenUrl10()
    {
        WebBinding.OpenWeb(WebType.BSD);
    }

    [RelayCommand]
    public void OpenUrl11()
    {
        WebBinding.OpenWeb(WebType.OpenFrpApi);
    }
}
