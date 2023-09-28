using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Info;

public partial class Info4Control : InfoControl
{
    public Info4Control()
    {
        InitializeComponent();

        DataContextChanged += Info4Control_DataContextChanged;
    }

    private void Info4Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info4Show")
        {
            Display();
        }
        else if (e.PropertyName == "Info4Close")
        {
            Close();
        }
    }
}
