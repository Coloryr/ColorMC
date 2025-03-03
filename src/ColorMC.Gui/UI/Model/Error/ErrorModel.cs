using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ColorMC.Gui.UI.Model.Error;

/// <summary>
/// 错误窗口
/// </summary>
public partial class ErrorModel : TopModel
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

    public ErrorModel(BaseModel model, string? data, Exception? e, bool close) : base(model)
    {
        _useName = ToString() ?? "ErrorModel";
        _text = new TextDocument($"{data ?? ""}{Environment.NewLine}" +
            $"{(e == null ? "" : e.ToString())}");

        NeedClose = close;

        Model.SetChoiseContent(_useName, App.Lang("ErrorWindow.Text1"), App.Lang("ErrorWindow.Text2"));
        Model.SetChoiseCall(_useName, Save, Push);
    }

    public ErrorModel(BaseModel model, string data, string e, bool close) : base(model)
    {
        _useName = ToString() ?? "ErrorModel";
        _text = new TextDocument($"{data}{Environment.NewLine}{e}");

        NeedClose = close;

        Model.SetChoiseContent(_useName, App.Lang("ErrorWindow.Text1"), App.Lang("ErrorWindow.Text2"));
        Model.SetChoiseCall(_useName, Save, Push);
    }

    /// <summary>
    /// 保存报错到文件
    /// </summary>
    public async void Save()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SaveFile(top, FileType.Text, [Text.Text]);
        if (res == true)
        {
            Model.Notify(App.Lang("ErrorWindow.Info1"));
        }
    }

    /// <summary>
    /// 上传报错
    /// </summary>
    public async void Push()
    {
        if (string.IsNullOrWhiteSpace(Text.Text))
        {
            Model.Show(App.Lang("GameLogWindow.Error2"));
            return;
        }
        var res = await Model.ShowAsync(App.Lang("GameLogWindow.Info4"));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameLogWindow.Info6"));
        var url = await WebBinding.PushMclo(Text.Text);
        Model.ProgressClose();
        if (url == null)
        {
            Model.Show(App.Lang("GameLogWindow.Error1"));
            return;
        }
        else
        {
            var top = Model.GetTopLevel();
            if (top == null)
            {
                return;
            }
            Model.InputWithChoise(string.Format(App.Lang("GameLogWindow.Info5"), url), App.Lang("GameLogWindow.Info8"), async () =>
            {
                await BaseBinding.CopyTextClipboard(top, url);
                Model.Notify(App.Lang("GameLogWindow.Info7"));
            });
            await BaseBinding.CopyTextClipboard(top, url);
            Model.Notify(App.Lang("GameLogWindow.Info7"));
        }
    }

    public override void Close()
    {
        Model.RemoveChoiseData(_useName);
    }
}
