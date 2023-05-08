using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls.Add;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColorMC.Gui.Objs;

public class DownloadModDisplayObj : INotifyPropertyChanged
{
    /// <summary>
    /// 名字
    /// </summary>
    private string name;
    private bool download;
    private int selectitem;

    public bool Optional;

    public List<string> ModVersion;
    public List<(DownloadItemObj Item, ModInfoObj Info)> Items;

    public List<string> Version { get { return ModVersion; } }
    public int SelectVersion { get { return selectitem; } set { selectitem = value; NotifyPropertyChanged(); } }
    public string Name { get { return name; } set { name = value; NotifyPropertyChanged(); } }
    public bool Download { get { return download; } set { download = value; NotifyPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
