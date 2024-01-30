using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

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

    private BaseModel _model;

    public ModExportModel(BaseModel model, string? pid, string? fid)
    {
        _pID = pid;
        _fID = fid;
        _model = model;

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
            return FuntionUtils.CheckNotNumber(PID) || FuntionUtils.CheckNotNumber(FID) ?
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

    partial void OnExportChanged(bool value)
    {
        if (value)
        {
            if (Obj1 == null)
            {
                _model.Show(App.Lang("Gui.Error52"));
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