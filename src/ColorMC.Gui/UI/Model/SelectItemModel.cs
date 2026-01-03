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
    private BoxShadows? _border = ThemeManager.BorderShadows;

    [ObservableProperty]
    private BoxShadows? _border1;
    [ObservableProperty]
    private IBrush _borderBrush;

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
        IsSelectChanged(value);

        ChangeBorder();
    }

    partial void OnTopChanged(bool value)
    {
        EnableButton = Top || IsSelect;

        ChangeBorder();
    }

    protected virtual void IsSelectChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }

    private void ChangeBorder()
    {
        if (IsSelect)
        {
            BorderBrush = ThemeManager.NowThemeColor.MainColor;
        }
        else
        {
            BorderBrush = ThemeManager.NowThemeColor.BorderColor;
        }

        if (Top)
        {
            Border1 = ThemeManager.BorderTopShadows;
        }
        else
        {
            Border1 = null;
        }
    }
}
