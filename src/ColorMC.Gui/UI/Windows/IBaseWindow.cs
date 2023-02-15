using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

public interface IBaseWindow
{
    public Info3Control Info3 { get; }
    public Info1Control Info1 { get; }
    public Info4Control Info { get; }
    public Info2Control Info2 { get; }
}
