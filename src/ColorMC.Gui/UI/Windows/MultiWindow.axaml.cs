using Avalonia.Controls;
using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 不带边框的多窗口
/// </summary>
public partial class MultiWindow : AMultiWindow
{
    public override TitleControl Head => MainView.TitleControl;

    public override int DefaultWidth => 760;
    public override int DefaultHeight => 450;

    public MultiWindow()
    {
        InitializeComponent();
    }

    public MultiWindow(BaseUserControl con)
    {
        InitializeComponent();

        InitMultiWindow(con);

        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = -1;
    }

    protected override void SetChild(Control control)
    {
        MainView.MainControl.Child = control;
    }
}
