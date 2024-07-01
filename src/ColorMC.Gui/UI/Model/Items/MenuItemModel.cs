using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 菜单项目
/// </summary>
public partial class MenuItemModel : ObservableObject
{
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; init; }
    /// <summary>
    /// 标题
    /// </summary>
    public string Text { get; init; }

    public int Index;

    /// <summary>
    /// 子项目
    /// </summary>
    public SubMenuItemModel[] SubMenu { get; init; }

    [ObservableProperty]
    private bool _isCheck;

    [RelayCommand]
    public void Select()
    {
        IsCheck = true;
    }
}

public partial class SubMenuItemModel : ObservableObject
{
    public string Name { get; init; }
    public Action Func { get; init; }
    public bool Hide { get; init; }

    [RelayCommand]
    public void Select()
    {
        Func();
    }
}
