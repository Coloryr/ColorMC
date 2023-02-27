using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System;
using System.IO;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GameControl : UserControl
{
    public GameSettingObj Obj { get; private set; }
    public GameControl()
    {
        InitializeComponent();
    }

    public void SetItem(GameSettingObj obj)
    {
        Obj = obj;
        Reload();
    }

    public void SetSelect(bool select)
    {
        Rectangle_Select.IsVisible = select;
    }

    public void SetLaunch(bool state)
    {
        Image2.IsVisible = state;
    }

    public void Reload()
    {
        TextBlock1.Text = Obj.Name;
        string file = Obj.GetIconFile();
        if (File.Exists(file))
        {
            Image1.Source = new Bitmap(file);
        }
        else
        {
            Image1.Source = App.GameIcon;
        }
    }
}
