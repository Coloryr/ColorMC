using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColorMC.Gui.Objs;

public record ServerPackModDisplayObj : INotifyPropertyChanged
{
    private string url;
    private string pid;
    private string fid;
    public bool Check { get; set; }
    public string FileName { get; set; }
    public string PID
    {
        get { return pid; }
        set { pid = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(Source)); }
    }
    public string FID
    {
        get { return fid; }
        set { fid = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(Source)); }
    }
    public string Source => Obj.Source;
    public string Sha1 { get; set; }
    public string Url { get { return url; } set { url = value; NotifyPropertyChanged(); } }

    public ModDisplayObj Obj;
    public ResourcepackDisplayObj Obj1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
