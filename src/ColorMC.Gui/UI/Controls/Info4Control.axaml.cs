using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls;

public partial class Info4Control : UserControl
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
            App.CrossFade300.Start(null, this);
        }
        else if (e.PropertyName == "Info4Close")
        {
            App.CrossFade300.Start(this, null);
        }
    }
}
