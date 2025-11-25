using System.Collections.Generic;
using System.Collections.ObjectModel;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 下载标记
/// </summary>
public abstract partial class AddBaseModel : TopModel, IAddControl
{
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
    /// 是否允许下一页
    /// </summary>
    [ObservableProperty]
    private bool _enableNextPage;

    /// <summary>
    /// 项目列表是否有下一页
    /// </summary>
    [ObservableProperty]
    private bool _nextPage;

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
    private int? _page = 0;
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
    /// 是否显示文件列表
    /// </summary>
    [ObservableProperty]
    private bool _displayVersion = false;

    /// <summary>
    /// 是否已经显示
    /// </summary>
    public bool Display { get; set; }

    /// <summary>
    /// 获取打开详情的时候的标题
    /// </summary>
    public abstract string Title { get; }

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
    /// 下载项目信息
    /// </summary>
    public AddFileInfoControlModel DisplayFile { get; init; }

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

    public AddBaseModel(BaseModel model) : base(model)
    {
        DisplayFile = new AddFileInfoControlModel(model, this);
    }

    /// <summary>
    /// 游戏列表显示
    /// </summary>
    /// <param name="value"></param>
    partial void OnDisplayVersionChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                DisplayVersion = false;
            });
        }
        else
        {
            Model.PopBack();
            Model.Title = Title;
        }
    }

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

        DisplayFile.GameVersionDownload = value;

        LoadDisplayList();
    }

    /// <summary>
    /// 页数改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageChanged(int? value)
    {
        if (!Display || _load)
        {
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
    /// 上一页
    /// </summary>
    public void Back()
    {
        if (_load || Page <= 0)
        {
            return;
        }

        Page -= 1;
    }

    /// <summary>
    /// 下一页
    /// </summary>
    [RelayCommand]
    public void Next()
    {
        if (_load)
        {
            return;
        }

        Page += 1;
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
        DisplayFile.Load(_lastSelect);
        DisplayVersion = true;
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

        foreach (var item in DisplayList)
        {
            if (item.FileType == info.Type && item.SourceType == info.Source
                && item.ID == info.PID)
            {
                item.NowDownload = true;
            }
        }
    }

    /// <summary>
    /// 下载项目结束
    /// </summary>
    /// <param name="info">下载项目</param>
    /// <param name="done">是否下载完成</param>
    public void StopDownload(FileItemDownloadModel info, bool done)
    {
        NowDownload.Remove(info);

        foreach (var item in DisplayList)
        {
            if (item.FileType == info.Type && item.SourceType == info.Source
                && item.ID == info.PID)
            {
                item.NowDownload = false;
                item.IsDownload = done;
            }
        }
    }

    /// <summary>
    /// 测试是否正在下载
    /// </summary>
    /// <param name="item">项目显示</param>
    public void TestFileItem(FileItemModel item)
    {
        foreach (var info in NowDownload)
        {
            if (item.FileType == info.Type && item.SourceType == info.Source
                && item.ID == info.PID)
            {
                item.NowDownload = true;
            }
        }
    }

    public override void Close()
    {
        ClearList();
    }
}
