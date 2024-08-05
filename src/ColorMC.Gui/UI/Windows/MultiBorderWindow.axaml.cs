using Avalonia;
using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

public partial class MultiBorderWindow : AMultiWindow
{
    public override HeadControl Head => HeadControl;

    public MultiBorderWindow()
    {
        InitializeComponent();
    }

    public MultiBorderWindow(BaseUserControl con)
    {
        InitializeComponent();

        PropertyChanged += OnPropertyChanged;

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.BorderOnly;
        }

        InitMultiWindow(con);
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainView.Margin = new Thickness(0);
            }
            else
            {
                MainView.Margin = new Thickness(10);
            }
        }
    }

    protected override void SetChild(Control control)
    {
        MainControl.Child = control;
    }
}
