using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Info;

public partial class Info5Control : InfoControl
{
    public Info5Control()
    {
        InitializeComponent();

        DataContextChanged += Info5Control_DataContextChanged;
    }

    private void Info5Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private async void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info5Show")
        {
            Display();
        }
        else if (e.PropertyName == "Info5Close")
        {
            await Close();
        }
    }
}
