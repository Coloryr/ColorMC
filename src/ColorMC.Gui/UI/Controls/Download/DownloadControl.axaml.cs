using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;

namespace ColorMC.Gui.UI.Controls.Download;

public partial class DownloadControl : BaseUserControl
{
    public DownloadControl()
    {
        InitializeComponent();

        Title = App.Lang("DownloadWindow.Title");
        UseName = ToString() ?? "DownloadControl";
    }

    public override void Closed()
    {
        WindowManager.DownloadWindow = null;
    }

    public override async Task<bool> Closing()
    {
        return DataContext is DownloadModel model && !await model.Stop();
    }

    public override TopModel GenModel(BaseModel model)
    {
        return new DownloadModel(model);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    public DownloadArg Start()
    {
        return (DataContext as DownloadModel)!.Start();
    }
}
