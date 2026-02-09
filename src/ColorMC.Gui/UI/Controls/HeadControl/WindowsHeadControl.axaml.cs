using Avalonia.Controls;

namespace ColorMC.Gui.UI.Controls.HeadControl;

public partial class WindowsHeadControl : UserControl
{
    public WindowsHeadControl()
    {
        InitializeComponent();

        Win32Properties.SetNonClientHitTestResult(ButtonClose, Win32Properties.Win32HitTestValue.Close);
        Win32Properties.SetNonClientHitTestResult(ButtonMin, Win32Properties.Win32HitTestValue.MinButton);
        Win32Properties.SetNonClientHitTestResult(ButtonMax, Win32Properties.Win32HitTestValue.MaxButton);
    }
}