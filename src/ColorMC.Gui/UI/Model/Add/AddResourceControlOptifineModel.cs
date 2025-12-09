using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Net.Apis;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏资源
/// 高清修复文件列表
/// </summary>
public partial class AddResourceControlModel : IAddOptifineControl
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
            Window.PushBack(back: () =>
            {
                OptifineDisplay = false;
            });
        }
        else
        {
            Window.PopBack();
            Type = 0;
            Source = 0;
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
        var dialog = Window.ShowProgress(LangUtils.Get("AddResourceWindow.Text21"));
        var list = await OptifineAPI.GetOptifineVersionAsync();
        Window.CloseDialog(dialog);
        _load = false;
        if (list == null)
        {
            Window.Show(LangUtils.Get("AddResourceWindow.Text33"));
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
        Window.Notify(LangUtils.Get("AddResourceWindow.Text24"));
    }

    /// <summary>
    /// 打开高清修复列表
    /// </summary>
    public async Task OptifineOpen()
    {
        if (GameVersionList.Contains(_obj.Version))
        {
            GameVersionOptifine = _obj.Version;
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
        OptifineItem?.IsSelect = false;
        OptifineItem = item;
        item.IsSelect = true;
    }

    /// <summary>
    /// 安装选中的optifine
    /// </summary>
    /// <param name="item"></param>
    public async void Install(OptifineVersionItemModel item)
    {
        var res = await Window.ShowChoice(string.Format(
            LangUtils.Get("AddResourceWindow.Text20"), item.Version));
        if (!res)
        {
            return;
        }
        var dialog = Window.ShowProgress(LangUtils.Get("Text.Downloading"));
        var res1 = await OptifineAPI.DownloadOptifineAsync(_obj, item.Obj);
        Window.CloseDialog(dialog);
        if (res1 == false)
        {
            Window.Show(LangUtils.Get("AddResourceWindow.Text35"));
        }
        else
        {
            Window.Notify(LangUtils.Get("Text.Downloaded"));
            OptifineDisplay = false;
        }
    }
}
