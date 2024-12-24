using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Count;

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

    public override TopModel GenModel(BaseModel model)
    {
        return new CountModel(model);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
