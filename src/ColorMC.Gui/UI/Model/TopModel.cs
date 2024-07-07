using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

public abstract partial class TopModel(BaseModel model) : ObservableObject
{
    public BaseModel Model => model;

    [ObservableProperty]
    private bool _minMode;

    public virtual void WidthChange(int index, double width)
    {
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
    /// 上层UI用关闭通知
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void WindowClose()
    {
        model.WindowClose();
    }
}
