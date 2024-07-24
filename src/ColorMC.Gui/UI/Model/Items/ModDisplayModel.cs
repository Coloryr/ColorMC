using System.Collections.Generic;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using CommunityToolkit.Mvvm.ComponentModel;

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
    public string Version => Obj.version + (IsNew ? " " + App.Lang("GameEditWindow.Tab4.Info21") : "");
    public string Local => Obj.Local;
    public string Author => MakeString(Obj.authorList);
    public string? Url => Obj.url;
    public string Loader => Obj.Loader.GetName();
    public string Source { get; init; }
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

    public ModDisplayModel(ModObj obj, ModInfoObj? obj1)
    {
        Obj = obj;
        Obj1 = obj1;

        Name = obj.ReadFail ? App.Lang("GameBinding.Info15") : obj.name;
        Enable = !obj.Disable;
        if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
        {
            Source = "";
        }
        else
        {
            Source = DownloadItemHelper.TestSourceType(PID, FID).GetName();
        }
    }

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

