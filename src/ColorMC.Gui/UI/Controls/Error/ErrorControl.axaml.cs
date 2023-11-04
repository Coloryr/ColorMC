using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Error;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("ErrorWindow.Title");

    private string? _data;
    private Exception? _e;
    private string _e1;
    private bool _close;
    private bool _type = false;

    public ErrorControl()
    {
        InitializeComponent();
    }

    public ErrorControl(string? data, Exception? e, bool close) : this()
    {
        _data = data;
        _e = e;
        _close = close;
        _type = true;
    }

    public ErrorControl(string data, string e, bool close) : this()
    {
        _data = data;
        _e1 = e;
        _close = close;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Closed()
    {
        if ((DataContext as ErrorModel)!.NeedClose
            || (App.IsHide && !BaseBinding.IsGameRuning()))
        {
            App.Close();
        }
    }

    public void SetBaseModel(BaseModel model)
    {
        if (_type)
        {
            DataContext = new ErrorModel(model, _data, _e, _close);
        }
        else
        {
            DataContext = new ErrorModel(model, _data ?? "", _e1, _close);
        }
    }
}
