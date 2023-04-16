using ColorMC.Core.Objs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public class DownloadModDisplayObj : INotifyPropertyChanged
{
    /// <summary>
    /// 名字
    /// </summary>
    private string name;
    private bool download;
    private int selectitem;

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
