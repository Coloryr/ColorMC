using ColorMC.Gui.UI.Model.Setting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加自定义DNS解析项目
/// </summary>
public partial class AddDnsModel : ObservableObject
{
    /// <summary>
    /// 是否为DNS
    /// </summary>
    [ObservableProperty]
    private bool _isDns = true;
    /// <summary>
    /// 是否为Https over dns
    /// </summary>
    [ObservableProperty]
    private bool _isHttps;
    /// <summary>
    /// 地址
    /// </summary>
    [ObservableProperty]
    private string _url;

    /// <summary>
    /// 确认
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(SettingModel.NameNetSetting, true);
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(SettingModel.NameNetSetting, false);
    }
}
