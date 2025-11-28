using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 控件模型
/// </summary>
/// <param name="window"></param>
public abstract partial class ControlModel(WindowModel window) : ObservableObject
{
    public const string WindowCloseName = "WindowClose";

    public WindowModel Window => window;

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
