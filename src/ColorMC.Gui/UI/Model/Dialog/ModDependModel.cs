using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class ModDependModel : ObservableObject
{
    /// <summary>
    /// Mod下载项目显示列表
    /// </summary>
    public readonly List<FileModVersionModel> _modList = [];
    /// <summary>
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<FileModVersionModel> DownloadModList { get; init; } = [];

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// 模组扩展选项文字
    /// </summary>
    public string ModDownloadText { get; init; }
    /// <summary>
    /// 是否为升级模式
    /// </summary>
    public bool IsUpgrade { get; init; }

    /// <summary>
    /// 展示所有附属模组
    /// </summary>
    [ObservableProperty]
    private bool _loadMoreMod;

    /// <summary>
    /// 下载的模组项目
    /// </summary>
    public DownloadModObj Modsave;
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType Source;

    private readonly string _window;

    public ModDependModel(string window, IEnumerable<FileModVersionModel> list, string name, string version, 
        DownloadModObj save, SourceType source)
    {
        IsUpgrade = false;
        _window = window;
        Source = source;
        Name = string.Format(LanguageUtils.Get("AddResourceWindow.Text38"), save.Info.Name);
        ModDownloadText = LanguageUtils.Get("AddResourceWindow.Text7");
        _modList.Add(new FileModVersionModel(name, [version], [save], false)
        {
            IsDisable = true,
            Download = true,
        });

        foreach (var item in list)
        {
            if (item.Optional == false)
            {
                item.Download = true;
            }

            _modList.Add(item);
        }

        ModsLoad();
    }

    public ModDependModel(string window, IEnumerable<FileModVersionModel> list)
    {
        IsUpgrade = true;
        _window = window;
        Name = LanguageUtils.Get("AddResourceWindow.Text39");
        ModDownloadText = LanguageUtils.Get("AddResourceWindow.Text15");
        foreach (var item in list)
        {
            item.Download = true;
            _modList.Add(item);
        }

        ModsLoad();
    }

    /// <summary>
    /// 加载更多模组
    /// </summary>
    /// <param name="value"></param>
    partial void OnLoadMoreModChanged(bool value)
    {
        ModsLoad(true);
    }

    /// <summary>
    /// 下载所选模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(_window, false, this);
    }

    /// <summary>
    /// 下载所选模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public void DownloadMod()
    {
        DialogHost.Close(_window, true, this);
    }

    /// <summary>
    /// 选择下载所有模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public void DownloadAllMod()
    {
        foreach (var item in DownloadModList)
        {
            item.Download = true;
        }

        DialogHost.Close(_window, true, this);
    }

    /// <summary>
    /// 加载可选模组列表
    /// </summary>
    public void ModsLoad(bool ischange = false)
    {
        DownloadModList.Clear();
        if (LoadMoreMod)
        {
            DownloadModList.AddRange(_modList);
        }
        else
        {
            foreach (var item in _modList)
            {
                if (!item.Optional)
                {
                    DownloadModList.Add(item);
                }
            }
            if (!ischange && DownloadModList.Count == 0)
            {
                LoadMoreMod = true;
            }
        }
    }
}
