using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System.IO;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GameControl : UserControl
{
    public GameSettingObj Obj { get; private set; }
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
        TextBlock1.TextWrapping = select ? TextWrapping.Wrap : TextWrapping.NoWrap;
    }

    public void SetLaunch(bool state)
    {
        Image2.IsVisible = state;

        ToolTip.SetTip(this, string.Format(App.GetLanguage("Tips.Text1"),
            Obj.LaunchData.AddTime.Ticks == 0 ? "" : Obj.LaunchData.AddTime.ToString(),
            Obj.LaunchData.LastTime.Ticks == 0 ? "" : Obj.LaunchData.LastTime.ToString()));
    }

    public void SetLoad(bool state)
    {
        Image3.IsVisible = state;
    }

    public void Reload()
    {
        TextBlock1.Text = Obj.Name;
        var file = Obj.GetIconFile();
        if (File.Exists(file))
        {
            Image1.Source = new Bitmap(file);
        }
        else
        {
            Image1.Source = App.GameIcon;
        }

        ToolTip.SetTip(this, string.Format(App.GetLanguage("Tips.Text1"), 
            Obj.LaunchData.AddTime.Ticks == 0 ? "" : Obj.LaunchData.AddTime.ToString(),
            Obj.LaunchData.LastTime.Ticks == 0 ? "" : Obj.LaunchData.LastTime.ToString()));
    }
}
