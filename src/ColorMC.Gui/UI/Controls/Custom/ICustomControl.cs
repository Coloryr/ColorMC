namespace ColorMC.Gui.UI.Controls.Custom;

public interface ICustomControl
{
    public string LauncherApi { get; }
    public BaseUserControl GetControl();
}
