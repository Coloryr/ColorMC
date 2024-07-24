using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ServerPackItemModel : ObservableObject
{
    [ObservableProperty]
    private string _url;
    [ObservableProperty]
    private string? _pID;
    [ObservableProperty]
    private string? fID;
    [ObservableProperty]
    public string _sha256;
    [ObservableProperty]
    public bool _check;
    [ObservableProperty]
    public string _fileName;

    public string Source
    {
        get
        {
            if (SourceType is { } type)
            {
                return type.GetName();
            }
            else
            {
                return "";
            }
        }
    }

    public SourceType? SourceType;

    public ModDisplayModel Mod;
    public ResourcepackObj Resourcepack;

    partial void OnPIDChanged(string? value)
    {
        Update();
    }

    partial void OnFIDChanged(string? value)
    {
        Update();
    }

    public void Update()
    {
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            SourceType = null;
        }
        else
        {
            SourceType = DownloadItemHelper.TestSourceType(PID, FID);
        }
    }
}
