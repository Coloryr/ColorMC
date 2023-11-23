using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tmds.DBus.SourceGenerator;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 文本获取 
/// </summary>
public class Localizer : INotifyPropertyChanged
{
    public readonly static Localizer Instance = new Localizer();

    public string this[string key]
    {
        get
        {
            return App.Lang(key);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Indexer.IndexerArrayName));
    }
}