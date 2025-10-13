using System.Collections.Generic;
using System.ComponentModel;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 带菜单窗口
/// </summary>
/// <param name="model">基础窗口</param>
public abstract partial class MenuModel(BaseModel model) : TopModel(model)
{
    public const string NameSideOpen = "SideOpen";
    public const string NameSideClose = "SideClose";
    public const string NameNowView = "NowView";

    /// <summary>
    /// 菜单项
    /// </summary>
    public List<MenuItemModel> TabItems { get; } = [];

    /// <summary>
    /// 显示的标题
    /// </summary>
    [ObservableProperty]
    private string _title;

    /// <summary>
    /// 切换目标视图
    /// </summary>
    [ObservableProperty]
    private int _nowView = -1;

    /// <summary>
    /// 是否切换到侧边栏模式
    /// </summary>
    [ObservableProperty]
    private bool _topSide;

    /// <summary>
    /// 菜单项切换
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    partial void OnNowViewChanged(int oldValue, int newValue)
    {
        CloseSide();
        if (oldValue != -1)
        {
            TabItems[oldValue].IsCheck = false;
        }
        TabItems[newValue].IsCheck = true;
        Title = TabItems[newValue].Text;
    }

    /// <summary>
    /// 窗口宽度切换
    /// </summary>
    /// <param name="index"></param>
    /// <param name="width"></param>
    public override void WidthChange(int index, double width)
    {
        if (index != 0)
        {
            return;
        }
        if (width < 450)
        {
            MinMode = true;
        }
        else
        {
            MinMode = false;
        }
    }

    /// <summary>
    /// 设置菜单
    /// </summary>
    /// <param name="items"></param>
    public void SetMenu(MenuItemModel[] items)
    {
        int a = 0;
        foreach (var item in items)
        {
            item.Index = a++;
            item.PropertyChanged += Item_PropertyChanged;
            TabItems.Add(item);
        }
    }

    /// <summary>
    /// 菜单项目切换
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is MenuItemModel model)
        {
            if (model.IsCheck)
            {
                NowView = model.Index;
            }
        }
    }

    /// <summary>
    /// 开启侧边栏
    /// </summary>
    [RelayCommand]
    public void OpenSide()
    {
        OnPropertyChanged(NameSideOpen);
    }

    /// <summary>
    /// 关闭侧边栏
    /// </summary>
    [RelayCommand]
    public void CloseSide()
    {
        OnPropertyChanged(NameSideClose);
    }
}
