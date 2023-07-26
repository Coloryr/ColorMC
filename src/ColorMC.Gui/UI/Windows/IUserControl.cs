using Avalonia.Controls;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public interface IUserControl
{
    public UserControl Con { get; }
    public IBaseWindow Window { get; }
    public string Title { get; }
    virtual public void Opened() { }
    virtual public void Closed() { }
    virtual public void Update() { }
    virtual public Task<bool> Closing() { return Task.Run(() => { return false; }); }
}
