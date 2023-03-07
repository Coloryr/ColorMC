using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Windows;

public class AddJavaWindow : SelfBaseWindow
{
    public AddJavaWindow()
    {
        Main = new AddJavaControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("AddJavaWindow.Title");
    }

    private new void Closed()
    {
        App.AddJavaWindow = null;
    }
}
