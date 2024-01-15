using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Download;

public partial class DownloadControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("DownloadWindow.Title");

    public string UseName { get; }

    private readonly ICollection<DownloadItemObj> _list;

    public DownloadControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "DownloadControl";
    }

    public DownloadControl(ICollection<DownloadItemObj> list) : this()
    {
        _list = list;
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
        var amodel = new DownloadModel(model, _list);
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

    public Task<bool> Start()
    {
        return (DataContext as DownloadModel)!.Start();
    }
}
