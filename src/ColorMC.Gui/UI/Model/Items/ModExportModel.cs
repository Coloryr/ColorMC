using System.ComponentModel;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Mod项目
/// </summary>
public partial class ModExportModel : ObservableObject
{
    [ObservableProperty]
    private bool _export;
    [ObservableProperty]
    private string? _pID;
    [ObservableProperty]
    private string? _fID;

    private readonly BaseModel _model;

    public ModExportModel(BaseModel model, string? pid, string? fid)
    {
        _pID = pid;
        _fID = fid;
        _model = model;

        if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
        {
            Source = null;
        }
        else
        {
            Source = DownloadItemHelper.TestSourceType(PID, FID);
        }

        Reload();
    }

    public string Name => Obj.Name;
    public string Modid => Obj.ModId;
    public string Loader => StringHelper.MakeString(Obj.Loaders);
    public SourceType? Source { get; init; }

    partial void OnPIDChanged(string? value)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Source)));
        Reload();
    }

    partial void OnFIDChanged(string? value)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Source)));
        Reload();
    }

    partial void OnExportChanged(bool value)
    {
        if (value)
        {
            if (Obj1 == null)
            {
                _model.Show(App.Lang("GameEditWindow.Tab4.Error5"));
                Export = false;
            }
        }
    }

    public void Reload()
    {
        if (Type == PackType.CurseForge)
        {
            Export = Source == SourceType.CurseForge;
        }
        else if (Type == PackType.Modrinth)
        {
            Export = Source != null;
        }
    }

    public string Sha1;
    public string Sha512;
    public string Url;
    public long FileSize;

    public PackType Type;
    public ModInfoObj? Obj1;
    public ModObj Obj;
}