using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Error;

public partial class ErrorModel : TopModel
{
    [ObservableProperty]
    private TextDocument _text;

    public bool NeedClose { get; }

    public ErrorModel(BaseModel model, string? data, Exception? e, bool close) : base(model)
    {
        _text = new TextDocument($"{data ?? ""}{Environment.NewLine}" +
            $"{(e == null ? "" : e.ToString())}");

        NeedClose = close;
    }

    public ErrorModel(BaseModel model, string data, string e, bool close) : base(model)
    {
        _text = new TextDocument($"{data}{Environment.NewLine}{e}");
        NeedClose = close;
    }

    [RelayCommand]
    public async Task Push()
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

    [RelayCommand]
    public async Task Save()
    {
        await PathBinding.SaveFile(FileType.Text, new[] { Text.Text });
    }

    protected override void Close()
    {

    }
}
