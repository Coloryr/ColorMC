using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record ModDisplayObj : INotifyPropertyChanged
{
    private bool disable { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Local { get; set; }
    public string Author { get; set; }
    public string Url { get; set; }
    public bool Enable
    {
        get { return !disable; }
        set { disable = value; NotifyPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
