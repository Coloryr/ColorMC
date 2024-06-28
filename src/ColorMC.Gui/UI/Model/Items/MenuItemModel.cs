using System;
using CommunityToolkit.Mvvm.ComponentModel;

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
    /// 子项目
    /// </summary>
    public SubMenuItemModel[] SubMenu { get; init; }

    [ObservableProperty]
    private bool _isCheck;
}

public record SubMenuItemModel
{
    public string Name { get; init; }
    public Action Func { get; init; }
}
