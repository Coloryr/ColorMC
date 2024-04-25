using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.UI.Windows;

public interface IUserControl
{
    public IBaseWindow Window { get; }
    public string Title { get; }
    public string UseName { get; }
    virtual public Bitmap GetIcon() { return App.GameIcon; }
    public void SetBaseModel(BaseModel model);
    virtual public void WindowStateChange(WindowState state) { }
    virtual public Task<bool> OnKeyDown(object? sender, KeyEventArgs e) { return Task.FromResult(false); }
    virtual public void IPointerPressed(PointerPressedEventArgs e) { }
    virtual public void IPointerReleased(PointerReleasedEventArgs e) { }
    virtual public void Opened() { }
    virtual public void Closed() { }
    virtual public void Update() { }
    virtual public Task<bool> Closing() { return Task.FromResult(false); }
}
