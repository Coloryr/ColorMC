using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.ServerPack;

namespace ColorMC.Gui.UI.Controls.ServerPack;

/// <summary>
/// 服务器实例生成窗口
/// </summary>
public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as ServerPackModel)?.ResourceItemEdit();
        });
    }
}
