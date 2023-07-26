using Avalonia;
using System.ComponentModel;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class StyleSel : INotifyPropertyChanged
{
    public readonly static StyleSel Instance = new StyleSel();

    private CornerRadius ButtonCornerRadius = new(3);

    public object? this[string key]
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerArrayName));
    }

    public void Load()
    {
        var config = GuiConfigUtils.Config.Style;

        ButtonCornerRadius = new(config.ButtonCornerRadius);

        Reload();
    }
}
