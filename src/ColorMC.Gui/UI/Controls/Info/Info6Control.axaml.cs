using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Info;

public partial class Info6Control : InfoControl
{
    public Info6Control()
    {
        InitializeComponent();

        DataContextChanged += Info6Control_DataContextChanged;
    }

    private void Info6Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private async void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info6Show")
        {
            Display();
        }
        else if (e.PropertyName == "Info6Close")
        {
            await Close();
        }
    }
}
