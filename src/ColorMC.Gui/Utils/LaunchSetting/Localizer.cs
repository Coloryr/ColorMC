using System.ComponentModel;

namespace ColorMC.Gui.Utils.LaunchSetting;

public class Localizer : INotifyPropertyChanged
{
    public static Localizer Instance { get; set; } = new Localizer();

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    public string this[string key]
    {
        get
        {
            if (App.Language != null && App.Language.TryGetValue(key, out var res1))
                return (res1 as string)!;

            return key;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Reload()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}