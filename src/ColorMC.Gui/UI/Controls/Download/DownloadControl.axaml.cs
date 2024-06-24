using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Download;

public partial class DownloadControl : BaseUserControl
{
    public DownloadControl()
    {
        InitializeComponent();

        Title = App.Lang("DownloadWindow.Title");
        UseName = ToString() ?? "DownloadControl";
    }

    public override void Opened()
    {
        Window.SetTitle(Title);
    }

    public override void Closed()
    {
        WindowManager.DownloadWindow = null;
    }

    public override async Task<bool> Closing()
    {
        return DataContext is DownloadModel model && !await model.Stop();
    }

    public override void SetModel(BaseModel model)
    {
        var amodel = new DownloadModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "WindowClose")
        {
            Window?.Close();
        }
    }

    public DownloadArg Start()
    {
        return (DataContext as DownloadModel)!.Start();
    }
}
