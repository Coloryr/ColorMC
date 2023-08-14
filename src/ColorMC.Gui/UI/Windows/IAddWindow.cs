using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Windows;

public interface IAddWindow
{
    public void SetSelect(FileItemModel item);
    public void Install(FileItemModel item);
    void Back();
    void Next();
}
