using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddJavaControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    private readonly AddJavaControlModel model;

    public AddJavaControl()
    {
        InitializeComponent();

        model = new(this);
        DataContext = model;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("AddJavaWindow.Title"));

        model.TypeIndex = 0;
    }

    public void Closed()
    {
        App.AddJavaWindow = null;
    }

    private void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not JavaDownloadDisplayObj obj)
            return;

        model.Install(obj);
    }
}
