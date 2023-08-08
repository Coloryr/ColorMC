using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

public partial class ServerPackModDisplayModel : ObservableObject
{
    [ObservableProperty]
    private string _url;
    [ObservableProperty]
    private string? _pID;
    [ObservableProperty]
    private string? fID;
    [ObservableProperty]
    public string sha1;
    [ObservableProperty]
    public bool check;
    [ObservableProperty]
    public string _fileName;

    public string Source
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
            {
                return "";
            }
            else if (FuntionUtils.CheckNotNumber(PID) || FuntionUtils.CheckNotNumber(FID))
            {
                return SourceType.Modrinth.GetName();
            }
            else
            {
                return SourceType.CurseForge.GetName();
            }
        }
    }

    public ModDisplayModel Mod;
    public ResourcepackDisplayObj Resourcepack;
}
