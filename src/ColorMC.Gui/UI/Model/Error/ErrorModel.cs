using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ColorMC.Gui.UI.Model.Error;

public partial class ErrorModel : TopModel
{
    [ObservableProperty]
    private TextDocument _text;

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

    public async void Save()
    {
        await PathBinding.SaveFile(FileType.Text, new[] { Text.Text });
    }

    public async void Push()
    {
        if (string.IsNullOrWhiteSpace(Text.Text))
        {
            Model.Show(App.Lang("GameLogWindow.Error2"));
            return;
        }
        var res = await Model.ShowWait(App.Lang("GameLogWindow.Info4"));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameLogWindow.Info6"));
        var url = await WebBinding.Push(Text.Text);
        Model.ProgressClose();
        if (url == null)
        {
            Model.Show(App.Lang("GameLogWindow.Error1"));
            return;
        }
        else
        {
            Model.ShowReadInfoOne(string.Format(App.Lang("GameLogWindow.Info5"), url), null);

            await BaseBinding.CopyTextClipboard(url);
            Model.Notify(App.Lang("GameLogWindow.Info7"));
        }
    }

    protected override void Close()
    {
        Model.RemoveChoiseData(_useName);
    }
}
