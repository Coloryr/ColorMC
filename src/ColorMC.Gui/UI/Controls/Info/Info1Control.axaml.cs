using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Info;

public partial class Info1Control : InfoControl
{
    public Info1Control()
    {
        InitializeComponent();

        DataContextChanged += Info1Control_DataContextChanged;
    }

    private void Info1Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private async void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info1Show")
        {
            Display();
        }
        else if (e.PropertyName == "Info1Close")
        {
            await Close();
        }
        else if (e.PropertyName == "Info1CloseAsync")
        {
            (DataContext as BaseModel)!.Info1Task = Close();
        }
    }
}
