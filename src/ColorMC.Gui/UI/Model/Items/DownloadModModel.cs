using System.Collections.Generic;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Mod下载项目显示
/// </summary>
public partial class FileModVersionModel : SelectItemModel
{
    [ObservableProperty]
    private bool _download;

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; init; }
    public int SelectVersion { get; set; }

    public List<string> Version { get; init; }

    /// <summary>
    /// 是否为可选
    /// </summary>
    public bool Optional;
    /// <summary>
    /// 下载项目列表
    /// </summary>
    public List<DownloadModArg> Items;
    /// <summary>
    /// 文件列表
    /// </summary>
    public List<FileVersionItemModel> FileItems;

    public FileModVersionModel(string name, List<FileVersionItemModel> version)
    {
        Download = false;
        Name = name;
        Version = [];
        SelectVersion = 0;
        FileItems = version;

        foreach (var item in version)
        {
            Version.Add(item.Name);
        }
    }

    public FileModVersionModel(string name, List<string> version, List<DownloadModArg> items, bool opt)
    {
        Download = false;
        Name = name;
        Version = version;
        Items = items;
        SelectVersion = 0;
        Optional = opt;
    }
}

public partial class ModUpgradeModel(ModObj obj, string name, List<string> version, List<DownloadModArg> items) : FileModVersionModel(name, version, items, false)
{
    public ModObj Obj => obj;
}
