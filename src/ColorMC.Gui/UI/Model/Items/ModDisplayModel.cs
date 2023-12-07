using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Mod项目
/// </summary>
public partial class ModDisplayModel : ObservableObject
{
    [ObservableProperty]
    private bool _enable;

    public string Name { get; init; }
    public string Modid => Obj.modid;
    public string Version => Obj.version + (IsNew ? " " + App.Lang("Gui.Info8") : "");
    public string Local => Obj.Local;
    public string Author => MakeString(Obj.authorList);
    public string? Url => Obj.url;
    public string Loader => Obj.Loader.GetName();
    public string Source
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
                return "";
            return FuntionUtils.CheckNotNumber(PID) || FuntionUtils.CheckNotNumber(FID) ?
                SourceType.Modrinth.GetName() : SourceType.CurseForge.GetName();
        }
    }

    public string? PID => Obj1?.ModId;
    public string? FID => Obj1?.FileId;

    public bool IsNew;
    /// <summary>
    /// Mod信息
    /// </summary>
    public ModInfoObj? Obj1;
    /// <summary>
    /// Mod内容
    /// </summary>
    public ModObj Obj;

    public void LocalChange()
    {
        OnPropertyChanged(nameof(Local));
    }

    private static string MakeString(List<string>? strings)
    {
        if (strings == null)
            return "";
        string temp = "";
        foreach (var item in strings)
        {
            temp += item + ",";
        }

        if (temp.Length > 0)
        {
            return temp[..^1];
        }

        return temp;
    }
}

