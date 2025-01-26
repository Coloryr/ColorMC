using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Collect;

namespace ColorMC.Gui.UI.Controls.Collect;

public partial class CollectControl : BaseUserControl
{
    public CollectControl()
    {
        InitializeComponent();

        Title = App.Lang("CollectWindow.Title");
        UseName = ToString() ?? "GameSettingObj";
    }

    public override TopModel GenModel(BaseModel model)
    {
        return new CollectModel(model);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}