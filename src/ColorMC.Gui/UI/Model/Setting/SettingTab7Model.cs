using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置页面
/// </summary>
public partial class SettingModel
{
    /// <summary>
    /// 图标
    /// </summary>
    public Bitmap Bitmap => ImageManager.GameIcon;
    /// <summary>
    /// 版本
    /// </summary>
    public string Version => ColorMCCore.Version;
    /// <summary>
    /// 运行方式
    /// </summary>
    public string RunType => GetRunType();

    /// <summary>
    /// 获取运行方式
    /// </summary>
    /// <returns></returns>
    private string GetRunType()
    {
        if (ColorMCGui.IsAot)
        {
            return LanguageUtils.Get("SettingWindow.Tab7.Text16");
        }
        else if (ColorMCGui.IsMin)
        {
            return LanguageUtils.Get("SettingWindow.Tab7.Text19");
        }
        return LanguageUtils.Get("SettingWindow.Tab7.Text17");
    }

    //打开网页
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
    public async Task OpenUrl5()
    {
        var res = await Model.ShowAsync(LanguageUtils.Get("SettingWindow.Tab7.Text18"));
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
