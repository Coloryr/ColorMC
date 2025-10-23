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

    /// <summary>
    /// 项目编号
    /// </summary>
    public int Index;

    /// <summary>
    /// 子项目
    /// </summary>
    public SubMenuItemModel[] SubMenu { get; init; }

    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    private bool _isCheck;

    [RelayCommand]
    public void Select()
    {
        IsCheck = true;
    }
}

/// <summary>
/// 子编号
/// </summary>
public partial class SubMenuItemModel : ObservableObject
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// 调用方法
    /// </summary>
    public Action Func { get; init; }
    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool Hide { get; init; }

    /// <summary>
    /// 选中后
    /// </summary>
    [RelayCommand]
    public void Select()
    {
        Func();
    }
}
