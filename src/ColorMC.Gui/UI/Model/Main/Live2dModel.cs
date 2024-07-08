using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    [ObservableProperty]
    private int _live2dWidth = 300;
    [ObservableProperty]
    private int _live2dHeight = 300;
    [ObservableProperty]
    private HorizontalAlignment _l2dPos = HorizontalAlignment.Right;
    [ObservableProperty]
    private VerticalAlignment _l2dPos1 = VerticalAlignment.Top;

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private bool _lowFps;

    /// <summary>
    /// 更换模型
    /// </summary>
    public void ChangeModel()
    {
        OnPropertyChanged("ModelChange");
    }

    /// <summary>
    /// 模型更换结束
    /// </summary>
    public void ChangeModelDone()
    {
        OnPropertyChanged("ModelChangeDone");
    }

    /// <summary>
    /// 删除模型
    /// </summary>
    public void DeleteModel()
    {
        OnPropertyChanged("ModelDelete");
    }

    /// <summary>
    /// 展示模型消息
    /// </summary>
    /// <param name="message"></param>
    public void ShowMessage(string message)
    {
        Message = message;
        OnPropertyChanged("ModelText");
    }
}
