using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class FontSel : INotifyPropertyChanged
{
    public static FontSel Instance { get; set; } = new FontSel();

    public static FontFamily Font = new(Program.Font);

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

    public void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }

    public static void Load()
    {
        if (!GuiConfigUtils.Config.FontDefault && FontManager.Current.GetInstalledFontFamilyNames()
                .Contains(GuiConfigUtils.Config.FontName))
        {
            Font = new(GuiConfigUtils.Config.FontName);
            Instance.Reload();
        }
        else
        {
            Font = new(Program.Font);
            Instance.Reload();
        }
    }
}
