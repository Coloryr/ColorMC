using Avalonia.Media;
using System.ComponentModel;
using System.Linq;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class FontSel : INotifyPropertyChanged
{
    public readonly static FontSel Instance = new FontSel();

    private static FontFamily s_font = new(ColorMCGui.Font);

    public FontFamily this[string key]
    {
        get
        {
            return s_font;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerArrayName));
    }

    public static string GetFont()
    {
        return s_font.Name;
    }

    public void Load()
    {
        if (!GuiConfigUtils.Config.FontDefault
            && !string.IsNullOrWhiteSpace(GuiConfigUtils.Config.FontName)
            && FontManager.Current.SystemFonts.Any(a => a.Name == GuiConfigUtils.Config.FontName))
        {
            s_font = new(GuiConfigUtils.Config.FontName);
            Reload();
        }
        else
        {
            s_font = new(ColorMCGui.Font);
            Reload();
        }
    }
}
