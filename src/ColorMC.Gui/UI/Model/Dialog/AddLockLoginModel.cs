using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加自定义登录模型锁定
/// </summary>
public partial class AddLockLoginModel : ObservableObject
{
    /// <summary>
    /// 模型列表
    /// </summary>
    public string[] Items { get; init; } = LanguageUtils.GetLockLoginType();

    /// <summary>
    /// 选中项目
    /// </summary>
    [ObservableProperty]
    private int _index;
    /// <summary>
    /// 是否启用文本
    /// </summary>
    [ObservableProperty]
    private bool _enableInput;
    /// <summary>
    /// 输入文本
    /// </summary>
    [ObservableProperty]
    private string _inputText;
    /// <summary>
    /// 输入文本
    /// </summary>
    [ObservableProperty]
    private string _inputText1;

    partial void OnIndexChanged(int value)
    {
        if (value == 0)
        {
            EnableInput = false;
            InputText = "";
            InputText1 = "";
        }
        else
        {
            EnableInput = true;
            InputText1 = "";
        }
    }

    /// <summary>
    /// 确认
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(SettingModel.NameLockLogin, true);
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(SettingModel.NameLockLogin, false);
    }
}
