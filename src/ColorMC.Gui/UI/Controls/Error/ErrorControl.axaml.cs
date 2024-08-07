using System;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Error;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : BaseUserControl
{
    private readonly string? _data;
    private readonly Exception? _e;
    private readonly string _e1;
    private readonly bool _close;
    private readonly bool _type = false;

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

    public override void Opened()
    {
        Window.SetTitle(Title);
    }

    public override void Closed()
    {
        if ((DataContext as ErrorModel)!.NeedClose
            || (App.IsHide && !GameManager.IsGameRuning()))
        {
            App.Close();
        }
    }

    public override void SetModel(BaseModel model)
    {
        ErrorModel amodel;
        if (_type)
        {
            amodel = new ErrorModel(model, _data, _e, _close);
        }
        else
        {
            amodel = new ErrorModel(model, _data ?? "", _e1, _close);
        }
        DataContext = amodel;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
