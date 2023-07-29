using Avalonia.Controls;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

public interface IBaseWindow
{
    public TopLevel? TopLevel { get; }
    public Info3Control InputInfo { get; }
    public Info1Control ProgressInfo { get; }
    public Info4Control OkInfo { get; }
    public Info2Control NotifyInfo { get; }
    public Info5Control ComboInfo { get; }
    public Info6Control TextInfo { get; }
    public HeadControl Head { get; }
    public IUserControl Con { get; }
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