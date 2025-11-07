using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.News;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.News;

/// <summary>
/// Minecraft News窗口
/// </summary>
public partial class MinecraftNewsControl : BaseUserControl
{
    public MinecraftNewsControl() : base(WindowManager.GetUseName<MinecraftNewsControl>())
    {
        InitializeComponent();

        Title = LanguageUtils.Get("NewsWindow.Title");
    }

    public override void Opened()
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
}
