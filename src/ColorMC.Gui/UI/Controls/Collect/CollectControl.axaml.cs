using System.ComponentModel;
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

    public override void Closed()
    {
        WindowManager.CollectWindow = null;
    }

    public override TopModel GenModel(BaseModel model)
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

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}