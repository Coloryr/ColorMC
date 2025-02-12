using System;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel
{
    public const string ModelChangeName = "ModelChange";
    public const string ModelChangeDoneName = "ModelChangeDone";
    public const string ModelDeleteName = "ModelDelete";
    public const string ModelTextName = "ModelText";

    /// <summary>
    /// Live2d宽度
    /// </summary>
    [ObservableProperty]
    private int _live2dWidth = 300;
    /// <summary>
    /// Live2d高度
    /// </summary>
    [ObservableProperty]
    private int _live2dHeight = 300;
    /// <summary>
    /// Live2d对其方式
    /// </summary>
    [ObservableProperty]
    private HorizontalAlignment _l2dPos = HorizontalAlignment.Right;
    /// <summary>
    /// Live2d对其方式
    /// </summary>
    [ObservableProperty]
    private VerticalAlignment _l2dPos1 = VerticalAlignment.Top;

    /// <summary>
    /// Live2d显示消息
    /// </summary>
    [ObservableProperty]
    private string _message;
    /// <summary>
    /// 是否低fps
    /// </summary>
    [ObservableProperty]
    private bool _lowFps;

    /// <summary>
    /// 更换模型
    /// </summary>
    public void ChangeModel()
    {
        OnPropertyChanged(ModelChangeName);
    }

    /// <summary>
    /// 模型更换结束
    /// </summary>
    public void ChangeModelDone()
    {
        OnPropertyChanged(ModelChangeDoneName);
    }

    /// <summary>
    /// 显示欢迎消息
    /// </summary>
    public void ModelShowHello()
    {
        var random = new Random();
        var index = random.Next(1000);
        if (index == 666)
        {
            L2dShowMessage("Ciallo～(∠·ω< )⌒★");
        }
        else
        {
            L2dShowMessage(App.Lang("Live2dControl.Text1"));
        }
    }

    /// <summary>
    /// 删除模型
    /// </summary>
    public void DeleteModel()
    {
        OnPropertyChanged(ModelDeleteName);
    }

    /// <summary>
    /// 展示模型消息
    /// </summary>
    /// <param name="message"></param>
    public void L2dShowMessage(string message)
    {
        Message = message;
        OnPropertyChanged(ModelTextName);
    }
}
