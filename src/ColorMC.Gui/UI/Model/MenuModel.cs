using System.Collections.Generic;
using System.ComponentModel;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model;

public abstract partial class MenuModel(BaseModel model) : TopModel(model)
{
    public const string SideOpen = "SideOpen";
    public const string SideClose = "SideClose";
    public const string NowViewName = "NowView";

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

    public bool IsWhell;

    private double _lastWheel;

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

    public void WhellChange(double dir)
    {
        _lastWheel += dir;
        if (_lastWheel < -2)
        {
            if (NowView < TabItems.Count - 1)
            {
                IsWhell = true;
                NowView++;
                IsWhell = false;
            }
            _lastWheel = 0;
        }
        else if (_lastWheel > 2)
        {
            if (NowView > 0)
            {
                IsWhell = true;
                NowView--;
                IsWhell = false;
            }
            _lastWheel = 0;
        }
    }

    /// <summary>
    /// 开启侧边栏
    /// </summary>
    [RelayCommand]
    public void OpenSide()
    {
        OnPropertyChanged("SideOpen");
    }

    /// <summary>
    /// 关闭侧边栏
    /// </summary>
    [RelayCommand]
    public void CloseSide()
    {
        OnPropertyChanged("SideClose");
    }
}
