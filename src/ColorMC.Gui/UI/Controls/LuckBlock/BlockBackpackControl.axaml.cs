using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.LuckBlock;

namespace ColorMC.Gui.UI.Controls.LuckBlock;

public partial class BlockBackpackControl : BaseUserControl
{
    public BlockBackpackControl() : base(WindowManager.GetUseName<BlockBackpackControl>())
    {
        InitializeComponent();

        Title = App.Lang("BlockBackpackWindow.Title");
    }

    public override void Opened()
    {
        if (DataContext is BlockBackpackModel model)
        {
            model.Load();
        }
    }

    public override void Closed()
    {
        WindowManager.BlockBackpackWindow = null;
    }

    public void Reload()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is BlockBackpackModel model)
            {
                model.Load();
            }
        });
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new BlockBackpackModel(model);
    }
}