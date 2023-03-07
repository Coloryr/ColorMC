using Avalonia.Controls;

namespace ColorMC.Gui.UI.Windows;

public interface IUserControl
{
    public UserControl Con { get; }
    public void Opened();
    public void Closed();
    public void Update();
    public void Closing();
}
