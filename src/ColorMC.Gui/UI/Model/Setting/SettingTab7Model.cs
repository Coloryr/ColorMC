using Avalonia.Media.Imaging;
using ColorMC.Core;
using ColorMC.Gui.Objs;
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
        WebBinding.OpenWeb(WebType.Web);
    }

    [RelayCommand]
    public void Open2()
    {
        WebBinding.OpenWeb(WebType.Minecraft);
    }

    [RelayCommand]
    public void Open3()
    {
        WebBinding.OpenWeb(WebType.Sponsor);
    }

    [RelayCommand]
    public void Open4()
    {
        WebBinding.OpenWeb(WebType.Github);
    }

    [RelayCommand]
    public void Open5()
    {
        WebBinding.OpenWeb(WebType.Guide);
    }

    [RelayCommand]
    public void Open6()
    {
        WebBinding.OpenWeb(WebType.Mcmod);
    }
}
