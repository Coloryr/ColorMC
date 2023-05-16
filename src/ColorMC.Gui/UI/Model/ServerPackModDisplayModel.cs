using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

public partial class ServerPackModDisplayModel : ObservableObject
{
    [ObservableProperty]
    private string url;
    [ObservableProperty]
    private string pID;
    [ObservableProperty]
    private string fID;
    [ObservableProperty]
    public string sha1;
    [ObservableProperty]
    public bool check;
    [ObservableProperty]
    public string fileName;

    public string Source
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
            {
                return "";
            }
            else if (Funtcions.CheckNotNumber(PID) || Funtcions.CheckNotNumber(FID))
            {
                return SourceType.Modrinth.GetName();
            }
            else
            {
                return SourceType.CurseForge.GetName();
            }
        }
    }

    public ModDisplayModel Obj;
    public ResourcepackDisplayObj Obj1;
}
