using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Error;

public partial class ErrorModel : BaseModel
{
    [ObservableProperty]
    private TextDocument _text;

    public bool NeedClose { get; }

    public ErrorModel(IUserControl con, string? data, Exception? e, bool close) : base(con)
    {
        _text = new TextDocument($"{data ?? ""}{Environment.NewLine}" +
            $"{(e == null ? "" : e.ToString())}");

        NeedClose = close;
    }

    public ErrorModel(IUserControl con, string data, string e, bool close) : base(con)
    {
        _text = new TextDocument($"{data}{Environment.NewLine}{e}");
        NeedClose = close;
    }

    [RelayCommand]
    public async Task Save()
    {
        await PathBinding.SaveFile(Window, FileType.Text, new[] { Text.Text });
    }

    public override void Close()
    {
        
    }
}
