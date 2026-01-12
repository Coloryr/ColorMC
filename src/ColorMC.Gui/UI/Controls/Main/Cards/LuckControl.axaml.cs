using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;

namespace ColorMC.Gui.UI.Controls.Main.Cards;

public partial class LuckControl : UserControl
{
    public LuckControl()
    {
        InitializeComponent();
    }

    private void LastControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty)
        {
            if (IsVisible == true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    CardAnimation.Start(this);
                });
            }
        }
    }
}