using System;
using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Error;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title { get; init; }

    private readonly string? _data;
    private readonly Exception? _e;
    private readonly string _e1;
    private readonly bool _close;
    private readonly bool _type = false;

    public string UseName { get; }

    public ErrorControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "ErrorControl";
    }

    public ErrorControl(string? data, Exception? e, bool close) : this()
    {
        _data = data;
        _e = e;
        _close = close;
        _type = true;

        Title = data ?? App.Lang("ErrorWindow.Title");
    }

    public ErrorControl(string data, string e, bool close) : this()
    {
        _data = data;
        _e1 = e;
        _close = close;

        Title = data;
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
