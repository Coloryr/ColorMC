using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加整合包
/// </summary>
public partial class AddModPackControlModel : TopModel, IAddControl
{
    /// <summary>
    /// 下载源列表
    /// </summary>
    public string[] SourceList { get; init; } = LanguageUtils.GetSourceList();

    /// <summary>
    /// 游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    /// <summary>
    /// 分类列表
    /// </summary>
    public ObservableCollection<string> CategorieList { get; init; } = [];
    /// <summary>
    /// 排序列表
    /// </summary>
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    /// <summary>
    /// 项目列表
    /// </summary>
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];

    /// <summary>
    /// 下载项目信息
    /// </summary>
    public AddFileInfoControlModel DisplayFile { get; init; }

    /// <summary>
    /// 分类
    /// </summary>
    private readonly Dictionary<int, string> _categories = [];
    /// <summary>
    /// 是否在加载
    /// </summary>
    private bool _load = false;
    /// <summary>
    /// 是否关闭
    /// </summary>
    private bool _close = false;

    /// <summary>
    /// 下载源
    /// </summary>
    [ObservableProperty]
    private int _source = -1;
    /// <summary>
    /// 分类
    /// </summary>
    [ObservableProperty]
    private int _categorie;
    /// <summary>
    /// 排序
    /// </summary>
    [ObservableProperty]
    private int _sortType;
    /// <summary>
    /// 当前页数
    /// </summary>
    [ObservableProperty]
    private int? _page = 0;
    /// <summary>
    /// 最大页数
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
    /// 是否选中了项目
    /// </summary>
    [ObservableProperty]
    private bool _isSelect = false;
    /// <summary>
    /// 是否没有项目
    /// </summary>
    [ObservableProperty]
    private bool _emptyDisplay = true;
    /// <summary>
    /// 是否下载源加载数据
    /// </summary>
    [ObservableProperty]
    private bool _sourceLoad;
    /// <summary>
    /// 是否允许下一页
    /// </summary>
    [ObservableProperty]
    private bool _enableNextPage;

    private FileItemModel? _last;

    /// <summary>
    /// 是否已经显示
    /// </summary>
    public bool Display { get; set; }

    /// <summary>
    /// 是否继续添加
    /// </summary>
    private bool _keep = false;
    /// <summary>
    /// 添加到的游戏分组
    /// </summary>
    private string? _group;

    private readonly string _useName;

    public AddModPackControlModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "AddModPackControlModel";
        DisplayFile = new AddFileInfoControlModel(model, this);
    }

    /// <summary>
    /// 分类改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnCategorieChanged(int value)
    {
        if (_load)
        {
            return;
        }

        Load();
    }

    /// <summary>
    /// 排序改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnSortTypeChanged(int value)
    {
        if (_load)
        {
            return;
        }

        Load();
    }

    /// <summary>
    /// 游戏版本改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionChanged(string? value)
    {
        if (_load)
        {
            return;
        }

        DisplayFile.GameVersionDownload = value;

        Load();
    }

    /// <summary>
    /// 页数改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageChanged(int? value)
    {
        if (_load)
            return;

        Load();
    }

    /// <summary>
    /// 下载源改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnSourceChanged(int value)
    {
        LoadSourceData();
    }

    /// <summary>
    /// 选中项目
    /// </summary>
    [RelayCommand]
    public void Select()
    {
        if (_last == null)
        {
            Model.Show(LanguageUtils.Get("AddModPackWindow.Text22"));
            return;
        }

        DisplayItems();
    }

    /// <summary>
    /// 刷新项目列表
    /// </summary>
    [RelayCommand]
    public void Reload()
    {
        if (!string.IsNullOrWhiteSpace(Text) && Page != 0)
        {
            Page = 0;
            return;
        }

        Load();
    }

    protected override void MinModeChange()
    {
        DisplayFile.MinMode = MinMode;
    }

    /// <summary>
    /// 加载搜索源数据
    /// </summary>
    public async void LoadSourceData()
    {
        if (_load)
        {
            return;
        }

        SourceLoad = false;
        _load = true;

        IsSelect = false;

        CategorieList.Clear();
        SortTypeList.Clear();

        GameVersionList.Clear();
        DisplayFile.GameVersionList.Clear();
        _categories.Clear();

        ClearList();

        switch (Source)
        {
            case 0:
            case 1:
                SortTypeList.AddRange(Source == 0 ?
                    LanguageUtils.GetCurseForgeSortTypes() :
                    LanguageUtils.GetModrinthSortTypes());

                Model.Progress(LanguageUtils.Get("AddModPackWindow.Text20"));
                var list = Source == 0 ?
                    await CurseForgeHelper.GetGameVersionsAsync() :
                    await ModrinthHelper.GetGameVersionAsync();
                var list1 = Source == 0 ?
                    await CurseForgeHelper.GetCategoriesAsync(FileType.ModPack) :
                    await ModrinthHelper.GetCategoriesAsync(FileType.ModPack);
                Model.ProgressClose();
                if (list == null || list1 == null)
                {
                    _load = false;
                    LoadFail();
                    return;
                }
                GameVersionList.AddRange(list);
                DisplayFile.GameVersionList.AddRange(list);

                _categories.Add(0, "");
                var a = 1;
                foreach (var item in list1)
                {
                    _categories.Add(a++, item.Key);
                }

                var list2 = new List<string>()
                {
                    ""
                };

                list2.AddRange(list1.Values);

                CategorieList.AddRange(list2);

                Categorie = 0;
                GameVersion = GameVersionList.FirstOrDefault();
                SortType = Source == 0 ? 1 : 0;

                Load();
                break;
        }

        SourceLoad = true;
        _load = false;
    }

    /// <summary>
    /// 选中项目
    /// </summary>
    /// <param name="last"></param>
    public void SetSelect(FileItemModel last)
    {
        IsSelect = true;
        _last?.IsSelect = false;
        _last = last;
        _last.IsSelect = true;
    }

    /// <summary>
    /// 开始安装项目
    /// </summary>
    private void DisplayItems()
    {
        DisplayFile.Load(_last);
        DisplayVersion = true;
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private async void Done(string? uuid)
    {
        Model.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Text29"));

        DisplayVersion = false;
        DisplayFile.Close();

        if (_keep)
        {
            return;
        }

        var model = WindowManager.MainWindow?.DataContext as MainModel;
        model?.Select(uuid);

        var res = await Model.ShowAsync(LanguageUtils.Get("AddGameWindow.Tab1.Text43"));
        if (res != true)
        {
            Dispatcher.UIThread.Post(WindowClose);
        }
        else
        {
            _keep = true;
        }
    }

    /// <summary>
    /// 加载搜索源信息失败
    /// </summary>
    private async void LoadFail()
    {
        var res = await Model.ShowAsync(LanguageUtils.Get("AddModPackWindow.Text25"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < SourceList.Length)
        {
            res = await Model.ShowAsync(LanguageUtils.Get("AddModPackWindow.Text21"));
            if (res)
            {
                Source++;
            }
        }
    }

    /// <summary>
    /// 加载项目列表
    /// </summary>
    private async void Load()
    {
        //MO不允许少文字搜索
        if (Source == 1 && Categorie == 4 && Text?.Length < 3)
        {
            Model.Show(LanguageUtils.Get("AddModPackWindow.Text27"));
            return;
        }

        Model.Progress(LanguageUtils.Get("AddModPackWindow.Text18"));
        var res = await WebBinding.GetModPackListAsync((SourceType)Source,
            GameVersion, Text, Page ?? 0, Source == 2 ? Categorie : SortType,
            Source == 2 ? "" : Categorie < 0 ? "" : _categories[Categorie]);

        //制作分页
        if (Source == 0)
        {
            MaxPage = res.Count / 20;
            EnableNextPage = (MaxPage - Page) > 0;
        }
        else
        {
            MaxPage = int.MaxValue;
            EnableNextPage = true;
        }

        var data = res.List;

        if (data == null)
        {
            Model.Show(LanguageUtils.Get("AddModPackWindow.Text23"));
            Model.ProgressClose();
            return;
        }

        DisplayList.Clear();

        //一页20
        int b = 0;
        for (int a = 0; a < data.Count; a++, b++)
        {
            if (b >= 20)
            {
                break;
            }
            var item = data[a];
            item.IsDownload = InstancesPath.Games.Any(item1 => item1.ModPack && item1.PID == item.Pid);
            item.Add = this;
            DisplayList.Add(item);
        }

        OnPropertyChanged(nameof(DisplayList));

        _last = null;

        EmptyDisplay = DisplayList.Count == 0;

        Model.ProgressClose();
        Model.Notify(LanguageUtils.Get("AddResourceWindow.Text24"));
    }

    /// <summary>
    /// 安装所选项目
    /// </summary>
    /// <param name="item"></param>
    public async void Install(FileItemModel item)
    {
        SetSelect(item);
        Model.Progress();
        var res = await WebBinding.GetFileListAsync(item.SourceType,
               item.Pid, 0, null, Loaders.Normal);
        Model.ProgressClose();
        if (res == null || res.List == null || res.List.Count == 0)
        {
            Model.Show(LanguageUtils.Get("AddModPackWindow.Text39"));
            return;
        }
        var item1 = res.List.First();
        if (item1.IsDownload)
        {
            var res1 = await Model.ShowAsync(LanguageUtils.Get("AddModPackWindow.Text40"));
            if (!res1)
            {
                return;
            }
        }

        Install(item1);
    }

    /// <summary>
    /// 清理项目列表
    /// </summary>
    private void ClearList()
    {
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
    }

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
    public void Next()
    {
        if (_load)
        {
            return;
        }

        Page += 1;
    }

    /// <summary>
    /// F5重载版本列表
    /// </summary>
    public void ReloadF5()
    {
        if (DisplayVersion)
        {
            LoadVersion();
        }
        else
        {
            Load();
        }
    }

    public override void Close()
    {
        if (DisplayVersion)
        {
            Model.PopBack();
        }

        _close = true;
        _load = true;
        Model.RemoveChoiseData(_useName);
        DisplayFile.FileList.Clear();
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
        _last = null;
    }

    /// <summary>
    /// 转到下载类型
    /// </summary>
    /// <param name="type">下载源</param>
    /// <param name="pid">项目ID</param>
    public async void GoFile(SourceType type, string pid)
    {
        Source = (int)type;
        await Task.Run(() =>
        {
            while ((!Display || _load) && !_close)
            {
                Thread.Sleep(100);
            }
        });

        _load = true;
        DisplayFile.PageDownload = 0;
        DisplayVersion = true;
        DisplayFile.LoadVersion(type, pid);
        _load = false;
    }

    public void SetGroup(string? group)
    {
        _group = group;
    }

    public void ShowInfo(FileItemModel item)
    {
        SetSelect(item);
        DisplayItems();
    }
}
