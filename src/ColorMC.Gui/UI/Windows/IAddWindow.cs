using ColorMC.Gui.UI.Controls.Add;

namespace ColorMC.Gui.UI.Windows;

public interface IAddWindow
{
    public void SetSelect(FileItemControl item);
    public void Install();
    void Back();
    void Next();
}
