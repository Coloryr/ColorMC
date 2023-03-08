using Avalonia.Controls;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

public interface IBaseWindow
{
    public Info3Control Info3 { get; }
    public Info1Control Info1 { get; }
    public Info4Control Info { get; }
    public Info2Control Info2 { get; }
    public Info5Control Info5 { get; }
    public Info6Control Info6 { get; }
    public HeadControl Head { get; }
    public UserControl Con { get; }
    public void SetTitle(string data);

    virtual public void Close()
    {
        if (ConfigBinding.WindowMode())
        {
            App.AllWindow?.Close(Con);
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
            App.AllWindow?.Add(Con);
        }
        else if (this is Window window)
        {
            window.Show();
        }
    }

    virtual public void Activate()
    {
        if (ConfigBinding.WindowMode())
        {
            App.AllWindow?.Active(Con);
        }
        else if (this is Window window)
        {
            window.Show();
        }
    }
}