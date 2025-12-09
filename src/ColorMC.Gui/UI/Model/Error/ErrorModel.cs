using System;
using System.Text;
using AvaloniaEdit.Document;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Error;

/// <summary>
/// 错误窗口
/// </summary>
public partial class ErrorModel : ControlModel
{
    /// <summary>
    /// 显示的文本
    /// </summary>
    [ObservableProperty]
    private TextDocument _text;

    /// <summary>
    /// 是否需要同时关闭启动器
    /// </summary>
    public bool NeedClose { get; }

    private readonly string _useName;

    private ErrorModel(WindowModel model) : base(model)
    {
        _useName = ToString() ?? "ErrorModel";
        Window.SetChoiseContent(_useName, LangUtils.Get("ErrorWindow.Text1"),
            LangUtils.Get("ErrorWindow.Text2"));
        Window.SetChoiseCall(_useName, Save, Push);
    }

    public ErrorModel(WindowModel model, string? log, Exception? e, bool close) : this(model)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(log))
        {
            builder.AppendLine(log);
        }
        if (e != null)
        {
            builder.Append(e.ToString());
        }
        _text = new TextDocument(builder.ToString());

        NeedClose = close;
    }

    public ErrorModel(WindowModel model, string log, bool close) : this(model)
    {
        _text = new TextDocument(log);

        NeedClose = close;
    }

    /// <summary>
    /// 保存报错到文件
    /// </summary>
    public async void Save()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SaveFileAsync(top, FileType.Text, [Text.Text]);
        if (res == true)
        {
            Window.Notify(LangUtils.Get("ErrorWindow.Text3"));
        }
    }

    /// <summary>
    /// 上传报错
    /// </summary>
    public async void Push()
    {
        if (string.IsNullOrWhiteSpace(Text.Text))
        {
            Window.Show(LangUtils.Get("GameLogWindow.Text26"));
            return;
        }
        var res = await Window.ShowChoice(LangUtils.Get("GameLogWindow.Text19"));
        if (!res)
        {
            return;
        }

        var dialog = Window.ShowProgress(LangUtils.Get("GameLogWindow.Text21"));
        var url = await McloAPI.PushAsync(Text.Text);
        Window.CloseDialog(dialog);
        if (url == null)
        {
            Window.Show(LangUtils.Get("GameLogWindow.Text25"));
            return;
        }
        else
        {
            var top = Window.GetTopLevel();
            if (top == null)
            {
                return;
            }
            var dialog1 = new InputModel(Window.WindowId)
            {
                Text1 = string.Format(LangUtils.Get("GameLogWindow.Text20"), url),
                ChoiseText = LangUtils.Get("GameLogWindow.Text23"),
                TextReadonly = true,
                ChoiseCall = () =>
                {
                    BaseBinding.CopyTextClipboard(top, url);
                    Window.Notify(LangUtils.Get("GameLogWindow.Text22"));
                }
            };
            Window.ShowDialog(dialog1);
            BaseBinding.CopyTextClipboard(top, url);
            Window.Notify(LangUtils.Get("GameLogWindow.Text22"));
        }
    }

    public override void Close()
    {
        Window.RemoveChoiseData(_useName);
    }
}
