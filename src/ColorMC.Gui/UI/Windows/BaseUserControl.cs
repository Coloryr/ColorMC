using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.UI.Windows;

public abstract class BaseUserControl : UserControl
{
    public IBaseWindow Window => WindowManager.FindRoot(VisualRoot);
    public string Title { get; protected set; }
    public string UseName { get; protected set; }
    public abstract Bitmap GetIcon();
    public  void SetBaseModel(BaseModel model)
    {
        model.PropertyChanged += Model_PropertyChanged;
        SetModel(model);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == BaseModel.WindowCloseName)
        {
            Window.Close();
        }
    }

    public abstract void SetModel(BaseModel model);
    public virtual void WindowStateChange(WindowState state) { }
    public virtual Task<bool> OnKeyDown(object? sender, KeyEventArgs e) 
    {
        return Task.FromResult(false); 
    }
    public virtual void IPointerPressed(PointerPressedEventArgs e) { }
    public virtual void IPointerReleased(PointerReleasedEventArgs e) { }
    public virtual void Opened() { }
    public virtual void Closed() { }
    public virtual void Update() { }
    public virtual Task<bool> Closing() { return Task.FromResult(false); }
}
