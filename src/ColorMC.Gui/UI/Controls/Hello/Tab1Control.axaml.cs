using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab1Control : UserControl
{
    public Tab1Control()
    {
        InitializeComponent();

        ComboBox1.ItemsSource = BaseBinding.GetLanguages();
        ComboBox1.SelectedIndex = 0;

        Button_Next.Click += Button_Next_Click;
        Button1.Click += Button1_Click;
        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as HelloControl)?.Done();
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var type = (LanguageType)ComboBox1.SelectedIndex;
        LanguageHelper.Change(type);
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as HelloControl)?.Next();
    }
}
