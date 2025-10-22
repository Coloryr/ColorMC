using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Net.Apis;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏资源
/// 高清修复文件列表
/// </summary>
public partial class AddControlModel : IAddOptifineControl
{
    /// <summary>
    /// 高清修复列表
    /// </summary>
    private readonly List<OptifineVersionItemModel> _optifineList = [];

    /// <summary>
    /// 显示的高清修复列表
    /// </summary>
    public ObservableCollection<OptifineVersionItemModel> DownloadOptifineList { get; init; } = [];

    /// <summary>
    /// 高清修复项目
    /// </summary>
    [ObservableProperty]
    private OptifineVersionItemModel? _optifineItem;

    /// <summary>
    /// 高清修复列表显示
    /// </summary>
    [ObservableProperty]
    private bool _optifineDisplay;
    /// <summary>
    /// 是否没有Optifine项目
    /// </summary>
    [ObservableProperty]
    private bool _emptyOptifineDisplay;

    /// <summary>
    /// 高清修复游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersionOptifine;

    /// <summary>
    /// 高清修复显示
    /// </summary>
    /// <param name="value"></param>
    partial void OnOptifineDisplayChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                OptifineDisplay = false;
            });
        }
        else
        {
            Model.PopBack();
            Type = 0;
            DownloadSource = 0;
        }
    }

    /// <summary>
    /// 高清修复游戏版本选择
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionOptifineChanged(string? value)
    {
        LoadOptifineVersion();
    }

    /// <summary>
    /// 刷新高清修复列表
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task LoadOptifineList()
    {
        _load = true;
        GameVersionList.Clear();
        _optifineList.Clear();
        DownloadOptifineList.Clear();
        Model.Progress(App.Lang("AddWindow.Info13"));
        var list = await OptifineAPI.GetOptifineVersionAsync();
        Model.ProgressClose();
        _load = false;
        if (list == null)
        {
            Model.Show(App.Lang("AddWindow.Error10"));
            return;
        }

        foreach (var item in list)
        {
            _optifineList.Add(new(item)
            {
                Add = this
            });
        }

        GameVersionList.Add("");
        GameVersionList.AddRange(from item2 in list
                                 group item2 by item2.MCVersion into newgroup
                                 select newgroup.Key);

        LoadOptifineVersion();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    /// <summary>
    /// 打开高清修复列表
    /// </summary>
    public async Task OptifineOpen()
    {
        if (GameVersionList.Contains(Obj.Version))
        {
            GameVersionOptifine = Obj.Version;
        }

        OptifineDisplay = true;
        await LoadOptifineList();
    }

    /// <summary>
    /// 加载高清修复列表
    /// </summary>
    public void LoadOptifineVersion()
    {
        DownloadOptifineList.Clear();
        var item = GameVersionOptifine;
        if (string.IsNullOrWhiteSpace(item))
        {
            DownloadOptifineList.AddRange(_optifineList);
        }
        else
        {
            DownloadOptifineList.AddRange(from item1 in _optifineList
                                          where item1.MCVersion == item
                                          select item1);
        }

        EmptyOptifineDisplay = DownloadOptifineList.Count == 0;
    }

    /// <summary>
    /// Optifine文件被选中
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(OptifineVersionItemModel item)
    {
        if (OptifineItem != null)
        {
            OptifineItem.IsSelect = false;
        }
        OptifineItem = item;
        item.IsSelect = true;
    }

    /// <summary>
    /// 安装选中的optifine
    /// </summary>
    /// <param name="item"></param>
    public async void Install(OptifineVersionItemModel item)
    {
        var res = await Model.ShowAsync(string.Format(
            App.Lang("AddWindow.Info10"), item.Version));
        if (!res)
        {
            return;
        }
        Model.Progress(App.Lang("AddWindow.Info11"));
        var res1 = await OptifineAPI.DownloadOptifineAsync(Obj, item.Obj);
        Model.ProgressClose();
        if (res1.State == false)
        {
            Model.Show(res1.Data!);
        }
        else
        {
            Model.Notify(App.Lang("Text.Downloaded"));
            OptifineDisplay = false;
        }
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

    /// <summary>
    /// 设置文件版本选中
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileVersionItemModel item)
    {
        if (File != null)
        {
            File.IsSelect = false;
        }
        File = item;
        item.IsSelect = true;
    }

    /// <summary>
    /// 升级所选的模组
    /// </summary>
    /// <param name="list"></param>
    public void Upgrade(ICollection<ModUpgradeModel> list)
    {
        IsUpgrade = true;
        if (ModDownloadDisplay)
        {
            CloseModDownloadDisplay();
        }
        _modList.Clear();
        _modList.AddRange(list);
        OpenModDownloadDisplay();
        _modList.ForEach(item =>
        {
            item.Download = true;
        });
        ModsLoad();
    }

    /// <summary>
    /// 打开模组文件列表
    /// </summary>
    private void OpenModDownloadDisplay()
    {
        ModDownloadDisplay = true;
        Model.PushBack(back: CloseModDownloadDisplay);
    }

    /// <summary>
    /// 关闭模组文件列表
    /// </summary>
    private void CloseModDownloadDisplay()
    {
        ModDownloadDisplay = false;
        Model.PopBack();
        DownloadModList.Clear();
    }
}
