using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Collect;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Collect;

/// <summary>
/// 资源收藏窗口
/// </summary>
public partial class CollectControl : BaseUserControl
{
    public CollectControl() : base(WindowManager.GetUseName<CollectControl>())
    {
        InitializeComponent();

        Title = LangUtils.Get("CollectWindow.Title");
    }

    public override void Closed()
    {
        WindowManager.CollectWindow = null;
    }

    protected override ControlModel GenModel(WindowModel model)
    {
        return new CollectModel(model);
    }
}