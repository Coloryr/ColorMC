using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    private bool IsClose;
    public ErrorControl()
    {
        InitializeComponent();
    }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("ErrorWindow.Title"));
    }

    public void Closed()
    {
        if (IsClose)
        {
            App.Close();
        }
    }

    public void Show(string data, Exception e, bool close)
    {
        //TextEditor1.Text = $"{data}{Environment.NewLine}{e}";
        IsClose = close;
    }

    public void Show(string data, string e, bool close)
    {
        //TextEditor1.Text = $"{data}{Environment.NewLine}{e}";
        IsClose = close;
    }
}
