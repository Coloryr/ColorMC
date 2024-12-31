using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;

namespace ColorMC.Gui.UI.Controls.Main.Cards;

public partial class NewsControl : UserControl
{
    public NewsControl()
    {
        InitializeComponent();

        //PropertyChanged += NewsControl_PropertyChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Dispatcher.UIThread.Post(() =>
        {
            CardAnimation.Start(this);
        });
    }

    //private void NewsControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    //{
    //    if (e.Property == IsVisibleProperty)
    //    {
    //        if (IsVisible == true)
    //        {
    //            Dispatcher.UIThread.Post(() =>
    //            {
    //                CardAnimation.Make().RunAsync(this);
    //            });
    //        }
    //    }
    //}
}