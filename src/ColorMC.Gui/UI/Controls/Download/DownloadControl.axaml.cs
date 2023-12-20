using Avalonia.Controls;
using ColorMC.Core;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Download;

public partial class DownloadControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("DownloadWindow.Title");

    public string UseName { get; }

    public DownloadControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "DownloadControl";
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        DataGrid1.SetFontColor();
    }

    public void Closed()
    {
        ColorMCCore.DownloadItemUpdate = null;

        App.DownloadWindow = null;
    }

    public void Load()
    {
        (DataContext as DownloadModel)!.Load();
    }

    public async Task<bool> Closing()
    {
        return DataContext is DownloadModel model && !await model.Stop();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new DownloadModel(model);
        DataContext = amodel;
    }
}
