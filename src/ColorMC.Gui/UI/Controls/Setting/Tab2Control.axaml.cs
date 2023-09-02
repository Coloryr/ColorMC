using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;
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
            var button = new Button()
            {
                Content = App.GetLanguage("SettingWindow.Tab2.Text43"),
                Height = 25,
                Width = 120
            };
            button.Click += Button_Click;
            PhoneSetting.Children.Add(button);
        }
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpenPhoneSetting();
    }

    public void OnDataContext_Change(object? sender, EventArgs e)
    {
        if (DataContext is SettingTab2Model model)
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
