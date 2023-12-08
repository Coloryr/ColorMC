using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model;

public abstract partial class MenuModel : TopModel
{
    /// <summary>
    /// 菜单项
    /// </summary>
    public abstract List<MenuObj> TabItems { get; init; }

    /// <summary>
    /// 显示的标题
    /// </summary>
    [ObservableProperty]
    private string _title;

    /// <summary>
    /// 切换目标视图
    /// </summary>
    [ObservableProperty]
    private int _nowView;

    public bool IsWhell;

    private double _lastWheel;

    public MenuModel(BaseModel model) : base(model)
    {
        Title = TabItems[0].Text;
    }

    partial void OnNowViewChanged(int value)
    {
        CloseSide();

        Title = TabItems[value].Text;
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
