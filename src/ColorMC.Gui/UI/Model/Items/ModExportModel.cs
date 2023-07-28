using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Model;

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

    public ModExportModel(string? pid, string? fid)
    {
        _pID = pid;
        _fID = fid;

        Reload();
    }

    public string Name => Obj.name;
    public string Modid => Obj.modid;
    public string Loader => Obj.Loader.GetName();
    public SourceType? Source
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
                return null;
            return Funtcions.CheckNotNumber(PID) || Funtcions.CheckNotNumber(FID) ?
                SourceType.Modrinth : SourceType.CurseForge;
        }
    }

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

    public void Reload()
    {
        if (Type == PackType.CurseForge)
        {
            Export = Source == SourceType.CurseForge;
        }
        else if (Type == PackType.Modrinth)
        {
            Export = Source == SourceType.Modrinth;
        }
    }

    public PackType Type;
    public ModInfoObj? Obj1;
    public ModObj Obj;
}