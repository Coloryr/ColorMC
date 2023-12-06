using CommunityToolkit.Mvvm.ComponentModel;
namespace ColorMC.Gui.UI.Model;

public abstract class TopModel(BaseModel model) : ObservableObject
{
    public BaseModel Model { get; } = model;

    public void TopClose()
    {
        Close();
    }
    protected abstract void Close();

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void WindowClose()
    {
        OnPropertyChanged("WindowClose");
    }
}
