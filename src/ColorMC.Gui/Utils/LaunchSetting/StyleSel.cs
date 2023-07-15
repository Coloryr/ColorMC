using Avalonia;
using Avalonia.Media;
using System.ComponentModel;
using System.Linq;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class StyleSel : INotifyPropertyChanged
{
    public static StyleSel Instance { get; set; } = new StyleSel();

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    private CornerRadius ButtonCornerRadius = new(3);

    public object this[string key]
    {
        get
        {
            if (key == "ButtonCornerRadius")
            {
                return ButtonCornerRadius;
            }
            return null;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }

    public void Load()
    {
        var config = GuiConfigUtils.Config.Style;

        ButtonCornerRadius = new(config.ButtonCornerRadius);

        Reload();
    }
}
