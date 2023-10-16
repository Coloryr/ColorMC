using Avalonia;
using System.ComponentModel;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 样式获取
/// </summary>
public class StyleSel : INotifyPropertyChanged
{
    public readonly static StyleSel Instance = new();

    private CornerRadius ButtonCornerRadius = new(3);
    private CornerRadius PicRadius = new(0);
    private int Radius;

    private static double FontTitleSize = 17;
    private static Thickness BorderPadding = new(6);

    public object? this[string key]
    {
        get
        {
            if (key == "ButtonCornerRadius")
            {
                return ButtonCornerRadius;
            }
            else if (key == "PicRadius")
            {
                return PicRadius;
            }
            else if (key == "FontTitle")
            {
                return FontTitleSize;
            }
            else if (key == "Radius")
            {
                return Radius;
            }
            else if (key == "BorderPadding")
            {
                return BorderPadding;
            }
            return null;
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
    /// 加载
    /// </summary>
    public void Load()
    {
        var config = GuiConfigUtils.Config.Style;

        ButtonCornerRadius = new(config.ButtonCornerRadius);

        if (config.EnablePicRadius)
        {
            PicRadius = new(config.ButtonCornerRadius);
        }
        else
        {
            PicRadius = new(0);
        }

        Radius = config.EnableBorderRadius ? config.ButtonCornerRadius : 0;

        Reload();
    }
}
