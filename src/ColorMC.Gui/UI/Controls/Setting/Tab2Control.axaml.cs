using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using Avalonia.Threading;

namespace ColorMC.Gui.UI.Controls.Setting;


public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();
    }

    public void Bind()
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
