using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;

namespace ColorMC.Gui.UI.Controls.Main.Cards;

/// <summary>
/// Æô¶¯Æ÷¸üÐÂ¿¨Æ¬
/// </summary>
public partial class UpdateControl : UserControl
{
    public UpdateControl()
    {
        InitializeComponent();

        PropertyChanged += UpdateControl_PropertyChanged;
    }

    private void UpdateControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
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