using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 控件模型
/// </summary>
/// <param name="window"></param>
public abstract partial class ControlModel(WindowModel window) : ObservableObject
{
    public const int MinModeWidth = 520;

    public const string WindowCloseName = "WindowClose";

    public const string MinModeName = nameof(MinMode);

    public WindowModel Window => window;

    /// <summary>
    /// 是否为小界面模式
    /// </summary>
    [ObservableProperty]
    private bool _minMode;

    partial void OnMinModeChanged(bool value)
    {
        MinModeChange();
    }

    protected virtual void MinModeChange()
    {

    }

    /// <summary>
    /// 界面宽度修改
    /// </summary>
    /// <param name="index"></param>
    /// <param name="width"></param>
    public virtual void WidthChange(int index, double width)
    {
        if (width < MinModeWidth)
        {
            MinMode = true;
        }
        else
        {
            MinMode = false;
        }
    }

    /// <summary>
    /// 上层UI用关闭通知
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void WindowClose()
    {
        OnPropertyChanged(WindowCloseName);
    }
}
