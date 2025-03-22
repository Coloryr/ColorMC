using System.ComponentModel;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Collect;

namespace ColorMC.Gui.UI.Controls.Collect;

/// <summary>
/// 资源收藏窗口
/// </summary>
public partial class CollectControl : BaseUserControl
{
    public CollectControl() : base(WindowManager.GetUseName<CollectControl>())
    {
        InitializeComponent();

        Title = App.Lang("CollectWindow.Title");
    }

    public override void Closed()
    {
        WindowManager.CollectWindow = null;
    }

    protected override TopModel GenModel(BaseModel model)
    {
        var model1 = new CollectModel(model);
        model1.PropertyChanged += Model1_PropertyChanged;
        return model1;
    }

    private void Model1_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is CollectModel model &&
            e.PropertyName == nameof(CollectModel.IsDownload))
        {
            if (model.IsDownload == true)
            {
                ThemeManager.CrossFade.Start(null, DownloadDisplay);
                ThemeManager.CrossFade.Start(ItemsView, null);
            }
            else
            {
                ThemeManager.CrossFade.Start(DownloadDisplay, null);
                ThemeManager.CrossFade.Start(null, ItemsView);
            }
        }
    }
}