using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls;

public partial class Info3Control : UserControl
{
    private bool _display = false;

    public Info3Control()
    {
        InitializeComponent();

        DataContextChanged += Info3Control_DataContextChanged;
        TextBox1.KeyDown += TextKeyDown;
        TextBox2.KeyDown += TextKeyDown;
    }

    private void Info3Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info3Show")
        {
            if (!_display)
            {
                _display = true;
                App.CrossFade300.Start(null, this);
            }
        }
        else if (e.PropertyName == "Info3Close")
        {
            if (_display)
            {
                _display = false;
                App.CrossFade300.Start(this, null);
            }
        }
    }

    private void TextKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            (DataContext as BaseModel)!.Info3Confirm();
        }
    }
}
