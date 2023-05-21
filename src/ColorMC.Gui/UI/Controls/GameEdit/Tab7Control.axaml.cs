using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using System;
using System.ComponentModel;
using System.Timers;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab7Control : UserControl
{
    public Tab7Control()
    {
        InitializeComponent();

        PointerWheelChanged += TextEditor1_PointerWheelChanged;
    }

    public void Bind()
    {
        (DataContext as GameEditTab7Model)!.PropertyChanged += Tab7Control_PropertyChanged;
    }

    private void Tab7Control_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Top")
        {
            Dispatcher.UIThread.Post(() =>
            {
                TextEditor1.ScrollToLine(TextEditor1.LineCount - 5);
            });
        }
    }

    private void TextEditor1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        (DataContext as GameEditTab7Model)!.SetNotAuto();
    }
}

