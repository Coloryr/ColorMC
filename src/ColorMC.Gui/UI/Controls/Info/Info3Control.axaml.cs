using Avalonia.Input;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Info;

public partial class Info3Control : InfoControl
{
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

    private async void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info3Show")
        {
            Display();
        }
        else if (e.PropertyName == "Info3Close")
        {
            await Close();
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
