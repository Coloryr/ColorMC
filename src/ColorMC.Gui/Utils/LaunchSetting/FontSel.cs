using Avalonia.Media;
using System.ComponentModel;
using System.Linq;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class FontSel : INotifyPropertyChanged
{
    public static FontSel Instance { get; set; } = new FontSel();

    private static FontFamily Font = new(ColorMCGui.Font);

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    public FontFamily this[string key]
    {
        get
        {
            return Font;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }

    public static string GetFont() 
    {
        return Font.Name;
    }

    public void Load()
    {
        if (!GuiConfigUtils.Config.FontDefault
            && !string.IsNullOrWhiteSpace(GuiConfigUtils.Config.FontName)
            && FontManager.Current.SystemFonts.Any(a => a.Name == GuiConfigUtils.Config.FontName))
        {
            Font = new(GuiConfigUtils.Config.FontName);
            Reload();
        }
        else
        {
            Font = new(ColorMCGui.Font);
            Reload();
        }
    }
}
