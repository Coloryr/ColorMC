using Avalonia.Controls;
using ColorMC.Core;
using System;

namespace ColorMC.UI;

public partial class MainWindow : Window
{
    public static MainWindow Window;
    public MainWindow()
    {
        Window = this;

        InitializeComponent();
        FontFamily = Program.FontFamily;
        Opened += MainWindow_Opened;
    }

    private void MainWindow_Opened(object? sender, System.EventArgs e)
    {
        CoreMain.Init(AppContext.BaseDirectory);
        App.ShowNew();
    }
}
