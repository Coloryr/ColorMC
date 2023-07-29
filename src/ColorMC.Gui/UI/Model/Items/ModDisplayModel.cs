using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// Mod项目
/// </summary>
public partial class ModDisplayModel : ObservableObject
{
    [ObservableProperty]
    private bool _enable;

    public string Name { get; set; }
    public string Modid => Obj.modid;
    public string Version => Obj.version + (IsNew ? " " + App.GetLanguage("Gui.Info8") : "");
    public string Local => Obj.Local;
    public string Author => Obj.authorList.MakeString();
    public string? Url => Obj.url;
    public string Loader => Obj.Loader.GetName();
    public string Source
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
                return "";
            return Funtcions.CheckNotNumber(PID) || Funtcions.CheckNotNumber(FID) ?
                SourceType.Modrinth.GetName() : SourceType.CurseForge.GetName();
        }
    }

    public string? PID => Obj1?.ModId;
    public string? FID => Obj1?.FileId;

    public bool IsNew;
    public ModInfoObj? Obj1;
    public ModObj Obj;

    public void LocalChange()
    {
        OnPropertyChanged(nameof(Local));
    }
}

