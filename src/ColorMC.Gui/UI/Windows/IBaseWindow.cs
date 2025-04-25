using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 实际窗口
/// </summary>
public interface IBaseWindow
{
    /// <summary>
    /// 窗口基础模型
    /// </summary>
    public BaseModel Model { get; }
    /// <summary>
    /// 显示内容
    /// </summary>
    public BaseUserControl ICon { get; }
    /// <summary>
    /// 设置窗口标题
    /// </summary>
    /// <param name="data"></param>
    public void SetTitle(string data);
    /// <summary>
    /// 设置图标
    /// </summary>
    /// <param name="icon"></param>
    public void SetIcon(Bitmap icon);
    /// <summary>
    /// 关闭窗口
    /// </summary>
    virtual public void Close()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Close(ICon);
        }
        else if (this is Window window)
        {
            window.Close();
        }
    }
    /// <summary>
    /// 显示窗口
    /// </summary>
    virtual public void Show()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Add(ICon);
        }
        else if (this is Window window)
        {
            window.Show();
        }
    }
    /// <summary>
    /// 转到最顶层
    /// </summary>
    virtual public void WindowActivate()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Active(ICon);
        }
        else if (this is Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Show();
            window.Activate();
        }
    }
    /// <summary>
    /// 重载图标
    /// </summary>
    void ReloadIcon();
}