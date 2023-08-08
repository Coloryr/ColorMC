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
    public IUserControl ICon { get; }
    public void SetTitle(string data);

    public void Close()
    {
        if (ConfigBinding.WindowMode())
        {
            App.AllWindow?.Close(ICon);
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
            App.AllWindow?.Add(ICon);
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
            App.AllWindow?.Active(ICon);
        }
        else if (this is Window window)
        {
            window.Show();
        }
    }
}