using Avalonia.Media.Imaging;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel : MenuModel
{
    public Bitmap Bitmap => App.GameIcon;
    public string Version => ColorMCCore.Version;

    [RelayCommand]
    public void OpenUrl1()
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
    public void OpenUrl5()
    {
        WebBinding.OpenWeb(WebType.Guide);
    }

    [RelayCommand]
    public void OpenUrl6()
    {
        WebBinding.OpenWeb(WebType.Mcmod);
    }
}
