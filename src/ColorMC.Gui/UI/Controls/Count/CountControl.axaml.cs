using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Count;

namespace ColorMC.Gui.UI.Controls.Count;

/// <summary>
/// 游戏统计窗口
/// </summary>
public partial class CountControl : BaseUserControl
{
    public CountControl() : base(WindowManager.GetUseName<CountControl>())
    {
        InitializeComponent();

        Title = App.Lang("CountWindow.Title");
    }

    public override void Closed()
    {
        WindowManager.CountWindow = null;
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new CountModel(model);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
