using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using System.IO;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GameControl : UserControl
{
    public GameSettingObj Obj { get; private set; }
    public GameControl()
    {
        InitializeComponent();

        Image1.Source = App.GameIcon;

        PointerEntered += GameControl_PointerEntered;
        PointerExited += GameControl_PointerExited;
    }

    private void GameControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void GameControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;

        SetTips();
    }

    public void SetItem(GameSettingObj obj)
    {
        Obj = obj;
        Reload();
    }

    public void SetSelect(bool select)
    {
        Rectangle1.IsVisible = select;
        TextBlock1.TextWrapping = select ? TextWrapping.Wrap : TextWrapping.NoWrap;

        SetTips();
    }

    public void SetLaunch(bool state)
    {
        Image2.IsVisible = state;

        SetTips();
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

        if (BaseBinding.IsGameRun(Obj))
        {
            SetLaunch(true);
        }

        SetTips();
    }

    private void SetTips()
    {
        ToolTip.SetTip(this, string.Format(App.GetLanguage("Tips.Text1"),
            Obj.LaunchData.AddTime.Ticks == 0 ? "" : Obj.LaunchData.AddTime.ToString(),
            Obj.LaunchData.LastTime.Ticks == 0 ? "" : Obj.LaunchData.LastTime.ToString(),
            Obj.LaunchData.GameTime.Ticks == 0 ? "" : Obj.LaunchData.GameTime.ToString(@"d\.hh\:mm\:ss")));
    }
}
