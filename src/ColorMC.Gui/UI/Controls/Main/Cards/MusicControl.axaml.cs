using Avalonia;
using Avalonia.Controls;
using ColorMC.Gui.UI.Animations;

namespace ColorMC.Gui.UI.Controls.Main.Cards;

/// <summary>
/// ���ֿ��ƿ�Ƭ
/// </summary>
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
                CardAnimation.Start(this);
            }
        }
    }
}