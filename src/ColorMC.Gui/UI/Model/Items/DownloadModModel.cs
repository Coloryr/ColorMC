﻿using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 模组下载项目显示
/// </summary>
public partial class FileModVersionModel : SelectItemModel
{
    /// <summary>
    /// 是否在下载
    /// </summary>
    [ObservableProperty]
    private bool _download;

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// 选择版本
    /// </summary>
    public int SelectVersion { get; set; }

    /// <summary>
    /// 版本列表
    /// </summary>
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

/// <summary>
/// 模组升级项目
/// </summary>
/// <param name="obj"></param>
/// <param name="name"></param>
/// <param name="version"></param>
/// <param name="items"></param>
public partial class ModUpgradeModel(ModObj obj, string name, List<string> version, List<DownloadModArg> items)
    : FileModVersionModel(name, version, items, false)
{
    /// <summary>
    /// 模组信息
    /// </summary>
    public ModObj Obj => obj;
}
