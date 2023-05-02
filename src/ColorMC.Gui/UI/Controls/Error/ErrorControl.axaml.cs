using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    private bool IsClose;
    public ErrorControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.SaveFile(Window, FileType.Text, new[] { TextEditor1.Text });
    }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("ErrorWindow.Title"));
    }

    public void Closed()
    {
        if (IsClose || (App.IsHide && !BaseBinding.IsGameRuning()))
        {
            App.Close();
        }
    }

    public void Show(string? data, Exception? e, bool close)
    {
        TextEditor1.Text = $"{data ?? ""}{Environment.NewLine}" +
            $"{(e == null ? "" : e.ToString())}";
        IsClose = close;
    }

    public void Show(string data, string e, bool close)
    {
        TextEditor1.Text = $"{data}{Environment.NewLine}{e}";
        IsClose = close;
    }
}
