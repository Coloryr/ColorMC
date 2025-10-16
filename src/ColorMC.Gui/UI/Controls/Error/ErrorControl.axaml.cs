using System;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Error;

namespace ColorMC.Gui.UI.Controls.Error;

/// <summary>
/// 错误窗口
/// </summary>
public partial class ErrorControl : BaseUserControl
{
    /// <summary>
    /// 错误信息
    /// </summary>
    private readonly string? _data;
    /// <summary>
    /// 异常
    /// </summary>
    private readonly Exception? _e;
    private readonly string _e1;
    /// <summary>
    /// 是否同时关闭启动器
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
