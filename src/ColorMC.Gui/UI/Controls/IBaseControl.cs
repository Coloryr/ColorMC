using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 基础页面
/// </summary>
public interface IBaseControl
{
    /// <summary>
    /// 窗口ID
    /// </summary>
    public string WindowId { get; }

    /// <summary>
    /// 页面窗口打开后
    /// </summary>
    public void ControlOpened();
    /// <summary>
    /// 页面窗口状态切换
    /// </summary>
    /// <param name="state">窗口状态</param>
    public void ControlStateChange(WindowState state);
    /// <summary>
    /// 页面键盘按键按下
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public Task<bool> OnKeyDown(object? sender, KeyEventArgs e);
}
