using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 添加自定义登录模型锁定
/// </summary>
public partial class AddLockLoginModel(string name) : BaseDialogModel(name)
{
    /// <summary>
    /// 模型列表
    /// </summary>
    public string[] Items { get; init; } = LangUtils.GetLockLoginType();

    /// <summary>
    /// 选中项目
    /// </summary>
    [ObservableProperty]
    public partial int Index { get; set; }

    /// <summary>
    /// 是否启用文本
    /// </summary>
    [ObservableProperty]
    public partial bool EnableInput { get; set; }

    /// <summary>
    /// 输入文本
    /// </summary>
    [ObservableProperty]
    public partial string InputText { get; set; }

    /// <summary>
    /// 输入文本
    /// </summary>
    [ObservableProperty]
    public partial string InputText1 { get; set; }

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
}
