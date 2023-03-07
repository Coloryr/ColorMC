using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Controls.Skin;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public class MainWindow : SelfBaseWindow
{
    public MainWindow()
    {
        Main = new MainControl();
        MainControl.Children.Add(Main.Con);
        SetTitle("MainWindow.Title");
    }
}
