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
    public partial BoxShadows? Border { get; set; } = ThemeManager.BorderShadows;

    [ObservableProperty]
    public partial IBrush BorderBrush { get; set; } = ThemeManager.NowThemeColor.BorderColor;

    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelect { get; set; }

    /// <summary>
    /// 是否鼠标在上面
    /// </summary>
    [ObservableProperty]
    public partial bool Top { get; set; }

    /// <summary>
    /// 是否启用按钮
    /// </summary>
    [ObservableProperty]
    public partial bool EnableButton { get; set; }

    protected virtual void OnTopChange() { }

    partial void OnIsSelectChanged(bool value)
    {
        IsSelectChanged(value);

        ChangeBorder();
    }

    partial void OnTopChanged(bool value)
    {
        EnableButton = Top || IsSelect;

        ChangeBorder();
        OnTopChange();
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
            Border = ThemeManager.BorderTopShadows;
        }
        else
        {
            Border = null;
        }
    }
}
