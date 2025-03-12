using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model;

namespace ColorMC.CustomGui;

public partial class UIControl : BaseUserControl
{
    public UIControl() : base(WindowManager.GetUseName<UIControl>())
    {
        InitializeComponent();

        Title = "服务器客户端"; //窗口标题
    }

    public override void Opened()
    {
        (DataContext as UIModel)?.Load();
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new UIModel(model);
    }
}
