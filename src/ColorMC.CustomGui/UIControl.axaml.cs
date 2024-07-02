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

        Title = "�������ͻ���"; //���ڱ���

        UseName = ToString() ?? "UIControl"; //�������Ҫ��
    }

    public override Bitmap GetIcon()
    {
        //����ͼ��
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
