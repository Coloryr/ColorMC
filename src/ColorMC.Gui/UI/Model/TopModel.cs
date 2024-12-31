using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

public abstract partial class TopModel(BaseModel model) : ObservableObject
{
    public const string WindowCloseName = "WindowClose";

    public const string MinModeName = nameof(MinMode);

    public BaseModel Model => model;

    [ObservableProperty]
    private bool _minMode;

    partial void OnMinModeChanged(bool value)
    {
        MinModeChange();
    }

    protected virtual void MinModeChange()
    { 
        
    }

    public virtual void WidthChange(int index, double width)
    {
        if (width < 470)
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
