using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab1Control : UserControl
{
    public Tab1Control()
    {
        InitializeComponent();

        Lang.Items = BaseBinding.GetLanguages();
        Lang.SelectedIndex = 0;

        Button_Next.Click += Button_Next_Click;
        Button1.Click += Button1_Click;
        Lang.SelectionChanged += Lang_SelectionChanged;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as HelloControl)?.Done();
    }

    private void Lang_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var type = (LanguageType)Lang.SelectedIndex;
        LanguageHelper.Change(type);
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as HelloControl)?.Next();
    }
}
