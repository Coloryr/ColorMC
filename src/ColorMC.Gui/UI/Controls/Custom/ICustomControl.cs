namespace ColorMC.Gui.UI.Controls.Custom;

/// <summary>
/// 自定义窗口
/// </summary>
public interface ICustomControl
{
    /// <summary>
    /// 启动器版本
    /// </summary>
    public string LauncherApi { get; }
    /// <summary>
    /// 获取控件
    /// </summary>
    /// <returns></returns>
    public BaseUserControl GetControl();
}
