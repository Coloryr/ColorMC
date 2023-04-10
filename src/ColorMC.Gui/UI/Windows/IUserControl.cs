using Avalonia.Controls;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public interface IUserControl
{
    public IBaseWindow Window { get; }
    virtual public void Opened() { }
    virtual public void Closed() { }
    virtual public void Update() { }
    virtual public Task Closing(WindowClosingEventArgs e) { return Task.CompletedTask; }
}
