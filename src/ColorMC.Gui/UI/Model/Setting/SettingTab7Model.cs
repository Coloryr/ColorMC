using Avalonia.Media.Imaging;
using ColorMC.Core;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab7Model : ObservableObject
{
    public Bitmap Bitmap => App.GameIcon;
    public string Version => ColorMCCore.Version;

    [RelayCommand]
    public void Open1()
    {
        BaseBinding.OpUrl("https://www.github.com/Coloryr/ColorMC");
    }

    [RelayCommand]
    public void Open2()
    {
        BaseBinding.OpUrl("https://www.minecraft.net/");
    }

    [RelayCommand]
    public void Open3()
    {
        BaseBinding.OpUrl("https://coloryr.github.io/sponsor.html");
    }

    [RelayCommand]
    public void Open4()
    {
        App.ShowCount();
    }

    [RelayCommand]
    public void Open5()
    {
        BaseBinding.OpUrl("https://github.com/Coloryr/ColorMC/blob/master/guide/Main.md");
    }

    [RelayCommand]
    public void Open6()
    {
        BaseBinding.OpUrl("https://github.com/Coloryr/ColorMC/blob/master/guide/Main.md");
    }
}
