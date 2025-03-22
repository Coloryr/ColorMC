using Avalonia.Media;
using ColorMC.Gui.Manager;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 带选中项目
/// </summary>
public partial class SelectItemModel : ObservableObject
{
    /// <summary>
    /// 选中框颜色
    /// </summary>
    [ObservableProperty]
    private BoxShadows _border = ThemeManager.BorderShadows;

    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    private bool _isSelect;
    /// <summary>
    /// 是否鼠标在上面
    /// </summary>
    [ObservableProperty]
    private bool _top;
    /// <summary>
    /// 是否启用按钮
    /// </summary>
    [ObservableProperty]
    private bool _enableButton;

    partial void OnIsSelectChanged(bool value)
    {
        if (IsSelect)
        {
            Border = ThemeManager.BorderSelecrShadows;
        }
        else
        {
            Border = ThemeManager.BorderShadows;
        }

        IsSelectChanged(value);
    }

    protected virtual void IsSelectChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }

    partial void OnTopChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }
}
