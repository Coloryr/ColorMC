using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    private bool Close;
    public ErrorControl()
    {
        InitializeComponent();
    }

    public UserControl Con => this;

    public void Closed()
    {
        if (Close)
        {
            App.Close();
        }
    }

    public void Closing()
    {

    }

    public void Opened()
    {

    }

    public void Show(string data, Exception e, bool close)
    {
        TextEditor1.Text = $"{data}{Environment.NewLine}{e}";
        Close = close;
    }

    public void Show(string data, string e, bool close)
    {
        TextEditor1.Text = $"{data}{Environment.NewLine}{e}";
    }

    public void Update()
    {

    }
}
