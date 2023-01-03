using Avalonia.Controls;
using System;

namespace ColorMC.Gui.UI;

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
        ShowDialog(MainWindow.Window);

        if (close)
        {
            App.Close();
        }
    }
}
