using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class Colors : INotifyPropertyChanged
{
    private static IBrush MainColor = Brush.Parse("#FF5ABED6");
    private static IBrush BackColor = Brush.Parse("#FFF4F4F5");
    private static IBrush Back1Color = Brush.Parse("#88FFFFFF");
    private static IBrush ButtonColor = Brush.Parse("#FF5EBDD3");

    public static Colors Instance { get; set; } = new Colors();

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

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

    public event PropertyChangedEventHandler PropertyChanged;

    public void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}
