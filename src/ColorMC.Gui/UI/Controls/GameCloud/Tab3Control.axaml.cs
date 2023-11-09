using Avalonia;
using Avalonia.Controls;
using ColorMC.Gui.UI.Model.GameCloud;

namespace ColorMC.Gui.UI.Controls.GameCloud;

public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        (DataContext as GameCloudModel)?.SetHeadBack();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        (DataContext as GameCloudModel)?.RemoveHeadBack();
    }
}
