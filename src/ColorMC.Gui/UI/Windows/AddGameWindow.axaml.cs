using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.FTB;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public class AddGameWindow : SelfBaseWindow
{
    public AddGameWindow()
    {
        Main = new AddGameControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("AddGameWindow.Title");
    }

    public  new void Closed()
    {
        App.AddGameWindow = null;
    }
}
