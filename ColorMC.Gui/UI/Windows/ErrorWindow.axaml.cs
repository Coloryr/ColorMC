using Avalonia.Controls;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;
    }

    public void Show(string data, Exception e, bool close)
    {
        Data.Text = $"{data}{Environment.NewLine}{e}";
        ShowDialog(App.MainWindow);

        if (close)
        {
            App.Close();
        }
    }
}
