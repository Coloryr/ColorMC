using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Setting;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Setting;


public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        DataContextChanged += OnDataContext_Change;
    }

    public void OnDataContext_Change(object? sender, EventArgs e)
    {
        (DataContext as SettingTab2Model)!.PropertyChanged += Tab2Control_PropertyChanged;
    }

    private void Tab2Control_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Hide")
        {
            Dispatcher.UIThread.Post(() =>
            {
                DropDownButton1.Flyout?.Hide();
            });
        }
    }
}
