using System;
using Avalonia;
using Avalonia.Controls;

namespace ColorMC.Gui.UI.Windows;

public partial class SingleLinuxWindow : ABaseWindow
{
    public override ITop ICon => Win;

    public SingleLinuxWindow()
    {
        InitializeComponent();

        Closed += UserWindow_Closed;
        Closing += SingleWindow_Closing;

        DataContext = Win.DataContext;

        PropertyChanged += OnPropertyChanged;

        SystemDecorations = SystemDecorations.BorderOnly;

        InitBaseWindow();
    }

    private async void SingleWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        var res = await Win.Closing();
        if (res)
        {
            e.Cancel = true;
        }
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        Win.Closed();
        App.Close();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            if (WindowState == WindowState.Maximized)
            {
                Win.Margin = new Thickness(0);
            }
            else
            {
                Win.Margin = new Thickness(10);
            }
        }
    }
}
