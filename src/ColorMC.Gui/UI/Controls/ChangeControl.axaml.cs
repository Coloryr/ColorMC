using System.Threading;
using Avalonia;
using Avalonia.Controls;
using ColorMC.Gui.Manager;

namespace ColorMC.Gui.UI.Controls;

public partial class ChangeControl : UserControl
{
    public static readonly StyledProperty<bool> IsHorizontalProperty =
        AvaloniaProperty.Register<ChangeControl, bool>(nameof(IsHorizontal));

    /// <summary>
    /// ÇÐ»»·½Ïò
    /// </summary>
    public bool IsHorizontal
    {
        get => GetValue(IsHorizontalProperty);
        set => SetValue(IsHorizontalProperty, value);
    }

    public ChangeControl()
    {
        InitializeComponent();
    }

    public void SwitchTo(bool dir, Control control, bool dir1, CancellationToken token)
    {
        if (!dir)
        {
            Content2.Child = control;
            if (IsHorizontal)
            {
                _ = ThemeManager.SelfPageSlideX.Start(Content1, Content2, dir1, token);
            }
            else
            {
                _ = ThemeManager.SelfPageSlideY.Start(Content1, Content2, dir1, token);
            }
        }
        else
        {
            Content1.Child = control;
            if (IsHorizontal)
            {
                _ = ThemeManager.SelfPageSlideX.Start(Content2, Content1, dir1, token);
            }
            else
            {
                _ = ThemeManager.SelfPageSlideY.Start(Content2, Content1, dir1, token);
            }
        }
    }
}