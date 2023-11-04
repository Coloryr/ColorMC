using Avalonia.Controls;
using ColorMC.Gui.UI.Model.NetFrp;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpTab3Control : UserControl
{
    public NetFrpTab3Control()
    {
        InitializeComponent();

        DataContextChanged += NetFrpTab3Control_DataContextChanged;
    }

    private void NetFrpTab3Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is NetFrpModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Insert")
        {
            TextEditor1.AppendText((DataContext as NetFrpModel)!.Temp ?? "");
        }
    }
}
