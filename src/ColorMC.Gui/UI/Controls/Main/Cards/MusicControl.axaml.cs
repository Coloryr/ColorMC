using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;

namespace ColorMC.Gui.UI.Controls.Main.Cards;

public partial class MusicControl : UserControl
{
    public MusicControl()
    {
        InitializeComponent();

        PropertyChanged += MusicControl_PropertyChanged;
    }

    private void MusicControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty)
        {
            if (IsVisible == true)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    CardAnimation.Make().RunAsync(this);
                });
            }
        }
    }
}