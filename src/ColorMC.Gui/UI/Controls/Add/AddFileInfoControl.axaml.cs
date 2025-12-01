using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Add;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddFileInfoControl : UserControl
{
    public AddFileInfoControl()
    {
        InitializeComponent();

        ModPackFiles.PointerPressed += ModPackFiles_PointerPressed;
        DataContextChanged += AddFileInfoControl_DataContextChanged;
    }

    private void AddFileInfoControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is AddBaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AddBaseModel.DisplayItemInfo))
        {
            ScrollViewer1.ScrollToHome();
            ScrollViewer2.ScrollToHome();
            ScrollViewer3.ScrollToHome();
        }
    }

    private void ModPackFiles_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddBaseModel)?.Download();
            e.Handled = true;
        }
    }
}