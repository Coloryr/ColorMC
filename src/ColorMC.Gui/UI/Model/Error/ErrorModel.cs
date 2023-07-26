using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace ColorMC.Gui.UI.Model.Error;

public partial class ErrorModel : ObservableObject
{
    private readonly IUserControl _con;

    [ObservableProperty]
    private TextDocument _text;

    public bool Close { get; }

    public ErrorModel(IUserControl con, string? data, Exception? e, bool close)
    {
        _text = new TextDocument($"{data ?? ""}{Environment.NewLine}" +
            $"{(e == null ? "" : e.ToString())}");

        _con = con;
        Close = close;
    }

    public ErrorModel(IUserControl con, string data, string e, bool close)
    {
        _text = new TextDocument($"{data}{Environment.NewLine}{e}");

        _con = con;
        Close = close;
    }

    [RelayCommand]
    public void Save()
    {
        BaseBinding.SaveFile(_con.Window, FileType.Text, new[] { Text.Text });
    }
}
