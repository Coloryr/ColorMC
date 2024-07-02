using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.CustomGui;

public partial class UIControl : BaseUserControl
{
    public UIControl()
    {
        InitializeComponent();

        Title = "服务器客户端"; //窗口标题

        UseName = ToString() ?? "UIControl"; //这个必须要有
    }

    public override Bitmap GetIcon()
    {
        //窗口图标
        return ImageManager.GameIcon;
    }

    public override void Opened()
    {
        (DataContext as UIModel)?.Load();
    }

    public override void SetModel(BaseModel model)
    {
        DataContext = new UIModel(model);
    }
}
