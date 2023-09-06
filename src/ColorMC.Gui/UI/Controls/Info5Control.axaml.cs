using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info5Control : UserControl
{
    private bool _display = false;

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

    private void Model_PropertyChanged(object? sender,  PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info5Show")
        {
            if (!_display)
            {
                _display = true;
                App.CrossFade300.Start(null, this);
            }
        }
        else if (e.PropertyName == "Info5Close")
        {
            if (_display)
            {
                _display = false;
                App.CrossFade300.Start(this, null);
            }
        }
    }
}
