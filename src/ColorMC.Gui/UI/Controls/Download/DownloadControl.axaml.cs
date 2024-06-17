using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.UI.Windows;

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
    }

    public void Closed()
    {
        App.DownloadWindow = null;
    }

    public async Task<bool> Closing()
    {
        return DataContext is DownloadModel model && !await model.Stop();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new DownloadModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;
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
