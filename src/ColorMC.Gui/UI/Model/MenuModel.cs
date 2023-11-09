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
    /// 目前视图
    /// </summary>
    [ObservableProperty]
    private int _nowView;

    /// <summary>
    /// 目前视图
    /// </summary>
    [ObservableProperty]
    private int _nowView1;

    public MenuModel(BaseModel model) : base(model)
    {
        Title = TabItems[0].Text;
    }

    partial void OnNowView1Changed(int value)
    {
        if (value != NowView)
        {
            NowView = value;
            OnPropertyChanged("Switch");
        }
    }

    /// <summary>
    /// 视图切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnNowViewChanged(int value)
    {
        CloseSide();

        Title = TabItems[NowView].Text;
        NowView1 = value;
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
