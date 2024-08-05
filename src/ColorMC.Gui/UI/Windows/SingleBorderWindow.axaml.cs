using System;
using Avalonia;
using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Windows;

public partial class SingleBorderWindow : ABaseWindow
{
    public override ITopWindow ICon => Win;

    public SingleBorderWindow()
    {
        InitializeComponent();

        Closed += UserWindow_Closed;
        Closing += SingleWindow_Closing;
        PropertyChanged += OnPropertyChanged;

        DataContext = Win.DataContext;

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.BorderOnly;
        }

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
