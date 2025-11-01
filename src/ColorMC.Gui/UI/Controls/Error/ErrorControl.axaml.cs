using System;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Error;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Error;

/// <summary>
/// 错误窗口
/// </summary>
public partial class ErrorControl : BaseUserControl
{
    /// <summary>
    /// 异常
    /// </summary>
    private readonly Exception? _e;
    private readonly string? _log;
    /// <summary>
    /// 是否同时关闭启动器
    /// </summary>
    private readonly bool _close;

    public ErrorControl() : base(WindowManager.GetUseName<ErrorControl>())
    {
        InitializeComponent();
    }

    public ErrorControl(string title, Exception? e, bool close) : this()
    {
        _e = e;
        _close = close;

        Title = title ?? LanguageUtils.Get("ErrorWindow.Title");
    }

    public ErrorControl(string title, string log, Exception? e, bool close) : this()
    {
        _log = log;
        _e = e;
        _close = close;

        Title = title ?? LanguageUtils.Get("ErrorWindow.Title");
    }

    public ErrorControl(string title, string log, bool close) : this()
    {
        _log = log;
        _close = close;

        Title = title ?? LanguageUtils.Get("ErrorWindow.Title");
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
        return new ErrorModel(model, _log, _e, _close);
    }
}
