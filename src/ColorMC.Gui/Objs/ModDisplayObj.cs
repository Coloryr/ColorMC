using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Core.Objs.Minecraft;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColorMC.Gui.Objs;

public record ModDisplayObj : INotifyPropertyChanged
{
    private bool disable { get; set; }
    public string Name { get; set; }
    public string Version => Obj.version;
    public string Local => Obj.Local;
    public string Author => Obj.authorList.MakeString();
    public string Url => Obj.url;
    public string Loader => Obj.Loader.GetName();
    public string Source
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
                return "";
            return UIUtils.CheckNotNumber(PID) || UIUtils.CheckNotNumber(FID) ?
                SourceType.Modrinth.GetName() : SourceType.CurseForge.GetName();
        }
    }
    public string PID { get; set; }
    public string FID { get; set; }

    public ModObj Obj;
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
