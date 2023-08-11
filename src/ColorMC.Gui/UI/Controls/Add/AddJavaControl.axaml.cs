using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddJavaControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("AddJavaWindow.Title");

    public BaseModel Model => _model;

    private readonly AddJavaControlModel _model;

    public AddJavaControl()
    {
        InitializeComponent();

        _model = new(this);
        DataContext = _model;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
    }

    public async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            await _model.Load();
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        _model.TypeIndex = 0;

        DataGrid1.SetFontColor();
    }

    public void Closed()
    {
        App.AddJavaWindow = null;
    }

    private void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not JavaDownloadObj obj)
            return;

        _model.Install(obj);
    }
}
