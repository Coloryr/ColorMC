using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class TabHelloControl : UserControl
{
    private HelloWindow Window;
    public TabHelloControl()
    {
        InitializeComponent();

        Lang.Items = OtherBinding.GetLanguages();
        Lang.SelectedIndex = 0;

        Button_Next.Click += Button_Next_Click;
        Lang.SelectionChanged += Lang_SelectionChanged;
    }

    private void Lang_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var type = (LanguageType)Lang.SelectedIndex;
        LanguageHelper.Change(type);
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Next();
    }
}
