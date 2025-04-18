using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;

namespace ColorMC.Gui.UI.Controls.Main.Cards;

/// <summary>
/// �ϴ�������Ƭ
/// </summary>
public partial class LastControl : UserControl
{
    public LastControl()
    {
        InitializeComponent();

        PropertyChanged += LastControl_PropertyChanged;
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