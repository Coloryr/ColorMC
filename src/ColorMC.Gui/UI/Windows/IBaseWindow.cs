using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

public interface IBaseWindow
{
    public BaseModel Model { get; }
    public BaseUserControl ICon { get; }
    public void SetTitle(string data);
    public void SetIcon(Bitmap icon);
    public void Close()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Close(ICon);
        }
        else if (this is Window window)
        {
            window.Close();
        }
    }

    virtual public void Show()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Add(ICon);
        }
        else if (this is Window window)
        {
            window.Show();
        }
    }

    virtual public void TopActivate()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Active(ICon);
        }
        else if (this is Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Show();
            window.Activate();
        }
    }

    virtual public void Hide()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Hide();
        }
        else if (this is Window window)
        {
            window.Hide();
        }
    }

    void SetSize(int width, int height);
}