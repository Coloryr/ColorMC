using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Error;

public partial class ErrorModel : ObservableObject
{
    private readonly IUserControl Con;
    [ObservableProperty]
    private TextDocument text;

    public bool Close { get; }

    public ErrorModel(IUserControl con, string? data, Exception? e, bool close)
    {
        text = new TextDocument($"{data ?? ""}{Environment.NewLine}" +
            $"{(e == null ? "" : e.ToString())}");

        Con = con;
        Close = close;
    }

    public ErrorModel(IUserControl con, string data, string e, bool close)
    {
        text = new TextDocument($"{data}{Environment.NewLine}{e}");

        Con = con;
        Close = close;
    }

    [RelayCommand]
    public void Save()
    {
        BaseBinding.SaveFile(Con.Window, FileType.Text, new[] { Text.Text });
    }
}
