using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddJavaControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("AddJavaWindow.Title");

    public string UseName { get; }

    public int NeedJava { get; set; }

    public AddJavaControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "AddJavaControl";

        JavaFiles.DoubleTapped += JavaFiles_DoubleTapped;
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddJavaControlModel)!.Load();
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        (DataContext as AddJavaControlModel)!.TypeIndex = 0;
    }

    public void Closed()
    {
        App.AddJavaWindow = null;
    }

    private void JavaFiles_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (JavaFiles.SelectedItem is not JavaDownloadObj obj)
            return;

        (DataContext as AddJavaControlModel)!.Install(obj);
    }

    public void SetBaseModel(BaseModel model)
    {
        DataContext = new AddJavaControlModel(model, NeedJava);
    }
}
