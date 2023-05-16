using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Model.Add;

namespace ColorMC.Gui.UI.Windows;

public interface IAddWindow
{
    public void SetSelect(FileItemModel item);
    public void Install(FileItemModel item);
    void Back();
    void Next();
}
