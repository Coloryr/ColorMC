using System;
using System.Text;
using AvaloniaEdit.Document;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

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

    private ErrorModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "ErrorModel";
        Model.SetChoiseContent(_useName, LanguageUtils.Get("ErrorWindow.Text1"), 
            LanguageUtils.Get("ErrorWindow.Text2"));
        Model.SetChoiseCall(_useName, Save, Push);
    }

    public ErrorModel(BaseModel model, string? log, Exception? e, bool close) : this(model)
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

    public ErrorModel(BaseModel model, string log, bool close) : this(model)
    {
        _text = new TextDocument(log);

        NeedClose = close;
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
        var res = await PathBinding.SaveFileAsync(top, FileType.Text, [Text.Text]);
        if (res == true)
        {
            Model.Notify(LanguageUtils.Get("ErrorWindow.Text3"));
        }
    }

    /// <summary>
    /// 上传报错
    /// </summary>
    public async void Push()
    {
        if (string.IsNullOrWhiteSpace(Text.Text))
        {
            Model.Show(LanguageUtils.Get("GameLogWindow.Text26"));
            return;
        }
        var res = await Model.ShowAsync(LanguageUtils.Get("GameLogWindow.Text19"));
        if (!res)
        {
            return;
        }

        Model.Progress(LanguageUtils.Get("GameLogWindow.Text21"));
        var url = await McloAPI.PushAsync(Text.Text);
        Model.ProgressClose();
        if (url == null)
        {
            Model.Show(LanguageUtils.Get("GameLogWindow.Text25"));
            return;
        }
        else
        {
            var top = Model.GetTopLevel();
            if (top == null)
            {
                return;
            }
            Model.InputWithChoise(string.Format(LanguageUtils.Get("GameLogWindow.Text20"), url),
                LanguageUtils.Get("GameLogWindow.Text23"), () =>
            {
                BaseBinding.CopyTextClipboard(top, url);
                Model.Notify(LanguageUtils.Get("GameLogWindow.Text22"));
            });
            BaseBinding.CopyTextClipboard(top, url);
            Model.Notify(LanguageUtils.Get("GameLogWindow.Text22"));
        }
    }

    public override void Close()
    {
        Model.RemoveChoiseData(_useName);
    }
}
