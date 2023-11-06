using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
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

        if (SystemInfo.Os == OsType.Android)
        {
            var con = ColorMCGui.PhoneGetSetting?.Invoke();
            if (con is Control con1)
            {
                PhoneSetting.Children.Add(con1);
            }
        }
    }

    public void OnDataContext_Change(object? sender, EventArgs e)
    {
        if (DataContext is SettingModel model)
        {
            model.PropertyChanged += Tab2Control_PropertyChanged;
        }
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
