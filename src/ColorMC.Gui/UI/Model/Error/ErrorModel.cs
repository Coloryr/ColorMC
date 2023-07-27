using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace ColorMC.Gui.UI.Model.Error;

public partial class ErrorModel : BaseModel
{
    [ObservableProperty]
    private TextDocument _text;

    public bool Close { get; }

    public ErrorModel(IUserControl con, string? data, Exception? e, bool close) : base(con)
    {
        _text = new TextDocument($"{data ?? ""}{Environment.NewLine}" +
            $"{(e == null ? "" : e.ToString())}");

        Close = close;
    }

    public ErrorModel(IUserControl con, string data, string e, bool close) : base(con)
    {
        _text = new TextDocument($"{data}{Environment.NewLine}{e}");
        ;
        Close = close;
    }

    [RelayCommand]
    public void Save()
    {
        BaseBinding.SaveFile(Window, FileType.Text, new[] { Text.Text });
    }
}
