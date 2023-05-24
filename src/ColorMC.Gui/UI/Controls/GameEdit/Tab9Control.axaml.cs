using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit.Items;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab9Control : UserControl
{
    public Tab9Control()
    {
        InitializeComponent();

        Button_C1.PointerExited += Button_C1_PointerLeave;
        Button_C.PointerEntered += Button_C_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;
    }

    private void Button_C1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_C1, null, CancellationToken.None);
    }

    private void Button_C_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_C1, CancellationToken.None);
    }

    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_R1, null, CancellationToken.None);
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_R1, CancellationToken.None);
    }
}
