using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System.IO;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GameControl : UserControl
{
    public GameSettingObj? Obj { get; private set; }
    public GameControl()
    {
        InitializeComponent();

        Image1.Source = App.GameIcon;
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

    public void SetLoad(bool state)
    {
        Image3.IsVisible = state;
    }

    public void Reload()
    {
        TextBlock1.Text = Obj?.Name;
        var file = Obj?.GetIconFile();
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
