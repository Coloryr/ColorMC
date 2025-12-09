using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Count;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Count;

/// <summary>
/// 游戏统计窗口
/// </summary>
public partial class CountControl : BaseUserControl
{
    public CountControl() : base(WindowManager.GetUseName<CountControl>())
    {
        InitializeComponent();

        Title = LangUtils.Get("CountWindow.Title");
    }

    public override void Closed()
    {
        WindowManager.CountWindow = null;
    }

    protected override ControlModel GenModel(WindowModel model)
    {
        return new CountModel(model);
    }
}
