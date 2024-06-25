using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Count;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Count;

public partial class CountControl : BaseUserControl
{
    public CountControl()
    {
        InitializeComponent();

        Title = App.Lang("CountWindow.Title");
        UseName = ToString() ?? "CountControl";
    }

    public override void Closed()
    {
        WindowManager.CountWindow = null;
    }

    public override void Opened()
    {
        Window.SetTitle(Title);
    }

    public override void SetModel(BaseModel model)
    {
        DataContext = new CountModel();
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
