using Avalonia.Media;
using ColorMC.Core;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class Colors : INotifyPropertyChanged
{
    public static readonly IBrush AppBackColor = Brush.Parse("#FFFFFFFF");
    public static readonly IBrush AppBackColor1 = Brush.Parse("#11FFFFFF");

    public static IBrush MainColor = Brush.Parse("#FF5ABED6");
    public static IBrush BackColor = Brush.Parse("#FFF4F4F5");
    public static IBrush Back1Color = Brush.Parse("#88FFFFFF");
    public static IBrush ButtonColor = Brush.Parse("#FF5EBDD3");

    public static Colors Instance { get; set; } = new Colors();

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    public static void Load() 
    {
        try
        {
            MainColor = Brush.Parse(GuiConfigUtils.Config.ColorMain);
            BackColor = Brush.Parse(GuiConfigUtils.Config.ColorBack);
            Back1Color = Brush.Parse(GuiConfigUtils.Config.ColorTranBack);

            Instance.Reload();
        }
        catch (Exception e)
        {
            Logs.Error("颜色数据读取失败", e);
        }
    }

    public IBrush this[string key]
    {
        get
        {
            if (key == "Main")
                return MainColor;
            else if (key == "Back")
                return BackColor;
            else if (key == "TranBack")
                return Back1Color;
            else if (key == "Button")
                return ButtonColor;

            return Brushes.White;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}
