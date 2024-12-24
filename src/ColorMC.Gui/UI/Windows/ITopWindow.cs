using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;

namespace ColorMC.Gui.UI.Windows;

public interface ITopWindow
{
    public string UseName { get; }

    public void TopOpened();
    public void WindowStateChange(WindowState state);
    public Task<bool> OnKeyDown(object? sender, KeyEventArgs e);
}
