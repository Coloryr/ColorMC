using Avalonia.Controls;
using System;

namespace ColorMC.UI;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();

        FontFamily = Program.FontFamily;
    }

    public void Show(string data, Exception e, bool close) 
    {
        Data.Text = $"{data}{Environment.NewLine}{e}";
        ShowDialog(MainWindow.Window);

        if (close)
        {
            App.Close();
        }
    }
}
