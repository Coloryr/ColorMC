using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.News;

namespace ColorMC.Gui.UI.Controls.News;

/// <summary>
/// Minecraft News´°¿Ú
/// </summary>
public partial class MinecraftNewsControl : BaseUserControl
{
    public MinecraftNewsControl() : base(WindowManager.GetUseName<MinecraftNewsControl>())
    {
        InitializeComponent();

        Title = App.Lang("NewsWindow.Title");
    }

    protected override void Opened()
    {
        if (DataContext is MinecraftNewsModel model)
        {
            model.LoadNews();
        }
    }

    public override void Closed()
    {
        WindowManager.NewsWindow = null;
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new MinecraftNewsModel(model);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
