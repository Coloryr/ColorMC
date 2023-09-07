using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls;

public partial class Info1Control : UserControl
{
    private bool _display = false;

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

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info1Show")
        {
            if (!_display)
            {
                _display = true;
                App.CrossFade300.Start(null, this);
            }
        }
        else if (e.PropertyName == "Info1Close")
        {
            if (_display)
            {
                _display = false;
                App.CrossFade300.Start(this, null);
            }
        }
        else if (e.PropertyName == "Info1CloseAsync")
        {
            if (_display)
            {
                _display = false;
                (DataContext as BaseModel)!.Info1Task
                    = App.CrossFade300.Start(this, null, CancellationToken.None);
            }
        }
    }
}
