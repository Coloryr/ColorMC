using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Model;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public interface IUserControl
{
    public IBaseWindow Window { get; }
    public string Title { get; }
    virtual public Bitmap GetIcon() { return App.GameIcon; }
    public void SetBaseModel(BaseModel model);
    virtual public void WindowStateChange(WindowState state) { }
    virtual public void OnKeyDown(object? sender, KeyEventArgs e) { }
    virtual public void Opened() { }
    virtual public void Closed() { }
    virtual public void Update() { }
    virtual public Task<bool> Closing() { return Task.Run(() => { return false; }); }
}
