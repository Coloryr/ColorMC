using Avalonia.Media;
using System.ComponentModel;
using System.Linq;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 字体
/// </summary>
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

    /// <summary>
    /// 刷新UI
    /// </summary>
    private void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerArrayName));
    }

    /// <summary>
    /// 获取字体名字
    /// </summary>
    /// <returns></returns>
    public static string GetFont()
    {
        return s_font.Name;
    }

    /// <summary>
    /// 加载字体
    /// </summary>
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
