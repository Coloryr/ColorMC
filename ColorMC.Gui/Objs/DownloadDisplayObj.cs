using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs;

public class DownloadDisplayObj : INotifyPropertyChanged
{
    private string name;
    private string allsize;
    private string nowsize;
    private string state;
    private int errortime;

    public string Name { get { return name; } set { name = value; NotifyPropertyChanged(); } }
    public string AllSize { get { return allsize; } set { allsize = value; NotifyPropertyChanged(); } }
    public string NowSize { get { return nowsize; } set { nowsize = value; NotifyPropertyChanged(); } }
    public string State { get { return state; } set { state = value; NotifyPropertyChanged(); } }
    public int ErrorTime { get { return errortime; } set { errortime = value; NotifyPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public long Last;
}
