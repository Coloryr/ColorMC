using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;

namespace ColorMC.Gui.UI.Controls.Main.Cards;

/// <summary>
/// Minecraft News¿¨Æ¬
/// </summary>
public partial class NewsControl : UserControl
{
    public NewsControl()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Dispatcher.UIThread.Post(() =>
        {
            CardAnimation.Start(this);
        });
    }
}