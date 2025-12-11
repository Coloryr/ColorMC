using System;
using System.ComponentModel;
using Avalonia.Controls;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Add;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddDownloadListControl : UserControl
{
    public AddDownloadListControl()
    {
        InitializeComponent();

        DataContextChanged += AddDownloadListControl_DataContextChanged;
    }

    private void AddDownloadListControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is AddBaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //if (e.PropertyName == nameof(AddBaseModel.DisplayDownload))
        //{
        //    if ((DataContext as AddBaseModel)!.DisplayDownload)
        //    {
        //        ThemeManager.CrossFade.Start(null, DownloadInfo);
        //    }
        //    else
        //    {
        //        ThemeManager.CrossFade.Start(DownloadInfo, null);
        //    }
        //}
    }
}