using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 下载标记
/// </summary>
public abstract partial class AddBaseModel(WindowModel model) : ControlModel(model), IAddControl
{
    private static readonly string[] s_displayStage = [".    ", "..   ", "...  ", ".... ", "....."];

    /// <summary>
    /// 下载源
    /// </summary>
    [ObservableProperty]
    private int _source = -1;
    /// <summary>
    /// 是否下载源加载数据
    /// </summary>
    [ObservableProperty]
    private bool _sourceLoad;
    /// <summary>
    /// 是否没有项目
    /// </summary>
    [ObservableProperty]
    private bool _emptyDisplay = true;
    /// <summary>
    /// 是否选择了项目
    /// </summary>
    [ObservableProperty]
    private bool _isSelect;

    /// <summary>
    /// 项目列表是否有下一页
    /// </summary>
    [ObservableProperty]
    private bool _nextPage;
    /// <summary>
    /// 是否有上一页
    /// </summary>
    [ObservableProperty]
    private bool _lastPage;

    /// <summary>
    /// 排序类型
    /// </summary>
    [ObservableProperty]
    private int _sortType = -1;
    /// <summary>
    /// 分类
    /// </summary>
    [ObservableProperty]
    private int _categorie;
    /// <summary>
    /// 项目当前页数
    /// </summary>
    [ObservableProperty]
    private int? _page = 1;
    /// <summary>
    /// 项目最大页数
    /// </summary>
    [ObservableProperty]
    private int _maxPage;

    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersion;
    /// <summary>
    /// 搜索文本
    /// </summary>
    [ObservableProperty]
    private string? _text;
    /// <summary>
    /// 下载文本
    /// </summary>
    [ObservableProperty]
    private string? _downloadText;
    /// <summary>
    /// 是否显示下载文本
    /// </summary>
    [ObservableProperty]
    private string? _displayText;

    /// <summary>
    /// 是否显示下载列表
    /// </summary>
    [ObservableProperty]
    private bool _displayDownload = false;
    /// <summary>
    /// 是否有下载项目
    /// </summary>
    [ObservableProperty]
    private bool _haveDownload = false;

    /// <summary>
    /// 是否已经显示
    /// </summary>
    public bool Display { get; set; }

    /// <summary>
    /// 显示的项目列表
    /// </summary>
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];
    /// <summary>
    /// 显示的游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    /// <summary>
    /// 显示的下载源列表
    /// </summary>
    public ObservableCollection<string> SourceList { get; init; } = [];
    /// <summary>
    /// 显示的排序列表
    /// </summary>
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    /// <summary>
    /// 显示的分类列表
    /// </summary>
    public ObservableCollection<string> CategorieList { get; init; } = [];

    /// <summary>
    /// 正在下载的项目
    /// </summary>
    public ObservableCollection<FileItemDownloadModel> NowDownload { get; init; } = [];

    /// <summary>
    /// 选中的下载项目
    /// </summary>
    protected FileItemModel? _lastSelect;
    /// <summary>
    /// 是否在加载
    /// </summary>
    protected bool _load = false;
    /// <summary>
    /// 是否关闭
    /// </summary>
    protected bool _close = false;

    /// <summary>
    /// 下载源列表
    /// </summary>
    protected readonly List<SourceType> SourceTypeList = [];

    private bool _isDisplayRun;
    private int _displayRunStage;

    /// <summary>
    /// 分类改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnCategorieChanged(int value)
    {
        if (!Display || _load)
        {
            return;
        }

        LoadDisplayList();
    }

    /// <summary>
    /// 排序改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnSortTypeChanged(int value)
    {
        if (!Display || _load)
        {
            return;
        }

        LoadDisplayList();
    }

    /// <summary>
    /// 游戏版本改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionChanged(string? value)
    {
        if (!Display || _load)
        {
            return;
        }

        LoadDisplayList();
    }

    /// <summary>
    /// 页数改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageChanged(int? value)
    {
        if (!Display || _load || value == null)
        {
            return;
        }
        if (value > MaxPage)
        {
            Page = MaxPage;
            return;
        }

        LoadDisplayList();
    }

    /// <summary>
    /// 下载源改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnSourceChanged(int value)
    {
        if (!Display || _load)
        {
            return;
        }
        LoadSourceData();
    }

    public abstract void LoadDisplayList();
    public abstract void LoadSourceData();

    public abstract void Install(FileVersionItemModel data);
    public abstract void Install(FileItemModel item);

    /// <summary>
    /// 获取项目列表
    /// </summary>
    [RelayCommand]
    public void Reload()
    {
        LoadDisplayList();
    }

    /// <summary>
    /// 根据名字刷新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GetNameList()
    {
        if (!string.IsNullOrWhiteSpace(Text) && Page != 1)
        {
            Page = 1;
            return;
        }

        LoadDisplayList();
    }

    /// <summary>
    /// 上一页
    /// </summary>
    [RelayCommand]
    public void LastListPage()
    {
        if (_load || Page <= 1)
        {
            return;
        }

        Page -= 1;
        PageLoad();
    }

    [RelayCommand]
    public void ShowDownload()
    {
        DisplayDownload = !DisplayDownload;
    }

    /// <summary>
    /// 下一页
    /// </summary>
    [RelayCommand]
    public void NextListPage()
    {
        if (_load)
        {
            return;
        }

        Page += 1;
        PageLoad();
    }

    /// <summary>
    /// 选中项目
    /// </summary>
    /// <param name="last"></param>
    public void SetSelect(FileItemModel last)
    {
        IsSelect = true;
        _lastSelect?.IsSelect = false;
        _lastSelect = last;
        _lastSelect.IsSelect = true;
    }

    /// <summary>
    /// 开始安装项目
    /// </summary>
    public void DisplayItems()
    {
        if (_lastSelect != null)
        {
            Last = _lastSelect;
            DisplayItemInfo = true;
        }
    }

    public void ShowInfo(FileItemModel item)
    {
        SetSelect(item);
        DisplayItems();
    }

    /// <summary>
    /// 清理项目列表
    /// </summary>
    public void ClearList()
    {
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
    }

    /// <summary>
    /// 开始下载项目
    /// </summary>
    /// <param name="info">下载项目</param>
    public void StartDownload(FileItemDownloadModel info)
    {
        NowDownload.Add(info);

        DownloadReload();
    }

    /// <summary>
    /// 下载项目结束
    /// </summary>
    /// <param name="info">下载项目</param>
    /// <param name="done">是否下载完成</param>
    public void StopDownload(FileItemDownloadModel info)
    {
        NowDownload.Remove(info);

        DownloadReload();
    }

    /// <summary>
    /// F5重载版本列表
    /// </summary>
    public void ReloadF5()
    {
        if (DisplayItemInfo)
        {
            LoadInfoVersion();
        }
        else
        {
            LoadDisplayList();
        }
    }

    public void Opened()
    {
        Display = true;
        Source = 0;
    }

    public override void Close()
    {
        ClearList();
        FileList.Clear();

        foreach (var item in NowDownload)
        {
            item.Window = null;
        }
        NowDownload.Clear();

        _isDisplayRun = false;
    }

    private void StartDisplay()
    {
        _isDisplayRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromSeconds(0.5));
    }

    private bool Run()
    {
        DisplayText = s_displayStage[_displayRunStage];
        _displayRunStage++;
        _displayRunStage %= s_displayStage.Length;
        return _isDisplayRun;
    }

    private void DownloadReload()
    {
        HaveDownload = NowDownload.Any();
        if (HaveDownload)
        {
            StartDisplay();
            DownloadText = string.Format(LangUtils.Get("AddModPackWindow.Text41"), NowDownload.Count);
        }
        else
        {
            DownloadText = LangUtils.Get("AddModPackWindow.Text45");
            _isDisplayRun = false;
        }
    }

    protected void PageLoad()
    {
        NextPage = Page < MaxPage;
        LastPage = Page > 1;
    }
}
