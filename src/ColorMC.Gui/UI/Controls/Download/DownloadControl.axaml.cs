using Avalonia.Controls;
using ColorMC.Core;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Download;

public partial class DownloadControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("DownloadWindow.Title");

    public DownloadControl()
    {
        InitializeComponent();
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        DataGrid1.SetFontColor();
    }

    public void Closed()
    {
        ColorMCCore.DownloadItemStateUpdate = null;

        App.DownloadWindow = null;
    }

    public void Load()
    {
        (DataContext as DownloadModel)!.Load();
    }

    public async Task<bool> Closing()
    {
        if (BaseBinding.IsDownload)
        {
            var res = await (DataContext as DownloadModel)!.Model.ShowWait(App.GetLanguage("DownloadWindow.Info4"));
            if (res)
            {
                BaseBinding.DownloadStop();
                return false;
            }
            return true;
        }

        return false;
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new DownloadModel(model);
        DataContext = amodel;
    }
}
