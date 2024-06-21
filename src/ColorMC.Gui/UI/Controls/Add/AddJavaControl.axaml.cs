using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddJavaControl : BaseUserControl
{
    public int NeedJava { get; set; }

    public AddJavaControl()
    {
        InitializeComponent();

        Title = App.Lang("AddJavaWindow.Title");
        UseName = ToString() ?? "AddJavaControl";

        JavaFiles.DoubleTapped += JavaFiles_DoubleTapped;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddJavaControlModel)!.Load();

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        (DataContext as AddJavaControlModel)!.TypeIndex = 0;
    }

    public override void Closed()
    {
        WindowManager.AddJavaWindow = null;
    }

    public override void SetBaseModel(BaseModel model)
    {
        DataContext = new AddJavaControlModel(model, NeedJava);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    private void JavaFiles_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (JavaFiles.SelectedItem is not JavaDownloadObj obj)
            return;

        (DataContext as AddJavaControlModel)!.Install(obj);
    }
}
