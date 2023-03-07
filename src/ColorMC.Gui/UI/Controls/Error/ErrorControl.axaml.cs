using Avalonia.Controls;
using ColorMC.Gui.UI.Windows;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    public ErrorControl()
    {
        InitializeComponent();
    }

    public UserControl Con => this;

    public void Closed()
    {
        
    }

    public void Closing()
    {
        
    }

    public void Opened()
    {
        
    }

    public void Show(string data, Exception e, bool close)
    {
        var window = (VisualRoot as Window)!;
        TextEditor1.Text = $"{data}{Environment.NewLine}{e}";

        if (close)
        {
            window.Show();
            App.Close();
        }
        else
        {
            window.Show();
        }
    }

    public void Show(string data, string e, bool close)
    {
        var window = (VisualRoot as Window)!;
        TextEditor1.Text = $"{data}{Environment.NewLine}{e}";

        if (close)
        {
            window.Show();
            App.Close();
        }
        else
        {
            window.Show();
        }
    }

    public void Update()
    {
        
    }
}
