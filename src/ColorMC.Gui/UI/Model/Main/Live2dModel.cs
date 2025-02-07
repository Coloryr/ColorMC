using System;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    public const string ModelChangeName = "ModelChange";
    public const string ModelChangeDoneName = "ModelChangeDone";
    public const string ModelDeleteName = "ModelDelete";
    public const string ModelTextName = "ModelText";

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
            ShowMessage("Ciallo～(∠·ω< )⌒★");
        }
        else
        {
            ShowMessage(App.Lang("Live2dControl.Text1"));
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
    public void ShowMessage(string message)
    {
        Message = message;
        OnPropertyChanged(ModelTextName);
    }
}
