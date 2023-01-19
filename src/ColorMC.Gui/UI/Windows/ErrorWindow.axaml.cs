using Avalonia.Controls;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();

        Icon = App.Icon;

        Rectangle1.MakeResizeDrag(this);
    }

    public void Show(string data, Exception e, bool close)
    {
        Data.Text = $"{data}{Environment.NewLine}{e}";

        if (close)
        {
            ShowDialog(App.MainWindow == null ? App.HelloWindow! : App.MainWindow);
            App.Close();
        }
        else
        {
            Show();
        }
    }
}
