using Avalonia.Controls;
using ColorMC.Core;
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

    private readonly DownloadModel _model;

    public DownloadControl()
    {
        InitializeComponent();

        _model = new(this);
        DataContext = _model;
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        DataGrid1.SetFontColor();
    }

    public void Closed()
    {
        _model.Close();

        ColorMCCore.DownloadItemStateUpdate = null;

        App.DownloadWindow = null;
    }

    public void Load()
    {
        _model.Load();
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            var res = await windows.OkInfo.ShowWait(App.GetLanguage("DownloadWindow.Info4"));
            if (res)
            {
                BaseBinding.DownloadStop();
                return false;
            }
            return true;
        }

        return false;
    }
}
