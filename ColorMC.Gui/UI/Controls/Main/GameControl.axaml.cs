using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
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

    public void Reload()
    {
        TextBlock1.Text = Obj.Name;
        if (!string.IsNullOrWhiteSpace(Obj.Image) && File.Exists(Obj.Image))
        {
            Image1.Source = new Bitmap(Obj.Image);
        }
        else
        {
            Image1.Source = App.GameIcon;
        }
    }
}
