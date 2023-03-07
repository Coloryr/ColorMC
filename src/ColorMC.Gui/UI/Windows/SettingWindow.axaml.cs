using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UI.Controls.User;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class SettingWindow : SelfBaseWindow
{
    public SettingWindow()
    {
        Main = new SettingControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("SettingWindow.Title");
    }

    private new void Closed()
    {
        App.SettingWindow = null;
    }

    public void GoTo(SettingType type)
    {
        (Main as SettingControl)?.GoTo(type);
    }
}
