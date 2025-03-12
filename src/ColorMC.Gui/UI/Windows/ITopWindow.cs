using Avalonia.Controls;
using Avalonia.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 基础窗口
/// </summary>
public interface ITopWindow
{
    /// <summary>
    /// 窗口ID
    /// </summary>
    public string WindowId { get; }

    /// <summary>
    /// 窗口打开后
    /// </summary>
    public void WindowOpened();
    /// <summary>
    /// 窗口状态切换
    /// </summary>
    /// <param name="state"></param>
    public void WindowStateChange(WindowState state);
    /// <summary>
    /// 窗口键盘按键按下
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public Task<bool> OnKeyDown(object? sender, KeyEventArgs e);
}
