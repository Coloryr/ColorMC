using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Error;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

/// <summary>
/// ���󴰿�
/// </summary>
public partial class ErrorControl : BaseUserControl
{
    /// <summary>
    /// ������Ϣ
    /// </summary>
    private readonly string? _data;
    /// <summary>
    /// �쳣
    /// </summary>
    private readonly Exception? _e;
    private readonly string _e1;
    /// <summary>
    /// �Ƿ�ͬʱ�ر�������
    /// </summary>
    private readonly bool _close;
    private readonly bool _type = false;

    public ErrorControl() : base(WindowManager.GetUseName<ErrorControl>())
    {
        InitializeComponent();
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

    public override void Closed()
    {
        if ((DataContext as ErrorModel)!.NeedClose
            || (App.IsHide && !GameManager.IsGameRuning()))
        {
            ColorMCGui.Exit();
        }
    }

    protected override TopModel GenModel(BaseModel model)
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
        return amodel;
    }
}
