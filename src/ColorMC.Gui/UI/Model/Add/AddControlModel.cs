using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddControlModel : GameModel, IAddOptifineWindow
{
    /// <summary>
    /// 显示的高清修复列表
    /// </summary>
    public ObservableCollection<OptifineVersionItemModel> DownloadOptifineList { get; init; } = [];
    /// <summary>
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<FileModVersionModel> DownloadModList { get; init; } = [];
    /// <summary>
    /// 显示的下载类型列表
    /// </summary>
    public string[] TypeList { get; init; } = LanguageBinding.GetAddType();
    /// <summary>
    /// 显示的游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    /// <summary>
    /// 显示的文件列表
    /// </summary>
    public ObservableCollection<FileVersionItemModel> FileList { get; init; } = [];
    /// <summary>
    /// 显示的项目列表
    /// </summary>
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];
    /// <summary>
    /// 显示的下载源列表
    /// </summary>
    public ObservableCollection<string> DownloadSourceList { get; init; } = [];
    /// <summary>
    /// 显示的排序列表
    /// </summary>
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    /// <summary>
    /// 显示的分类列表
    /// </summary>
    public ObservableCollection<string> CategorieList { get; init; } = [];

    /// <summary>
    /// 高清修复项目
    /// </summary>
    [ObservableProperty]
    private OptifineVersionItemModel? _optifineItem;
    /// <summary>
    /// 项目
    /// </summary>
    [ObservableProperty]
    private FileVersionItemModel? _file;
    /// <summary>
    /// 下载的模组
    /// </summary>
    [ObservableProperty]
    private FileModVersionModel? _mod;

    /// <summary>
    /// 是否在下载
    /// </summary>
    [ObservableProperty]
    private bool _isDownload;
    /// <summary>
    /// 是否没有项目
    /// </summary>
    [ObservableProperty]
    private bool _emptyDisplay = true;
    /// <summary>
    /// 是否没有项目
    /// </summary>
    [ObservableProperty]
    private bool _emptyVersionDisplay = true;
    /// <summary>
    /// 高清修复列表显示
    /// </summary>
    [ObservableProperty]
    private bool _optifineDisplay;
    /// <summary>
    /// mod列表显示
    /// </summary>
    [ObservableProperty]
    private bool _modDownloadDisplay;
    /// <summary>
    /// 文件列表显示
    /// </summary>
    [ObservableProperty]
    private bool _versionDisplay;
    /// <summary>
    /// 展示所有附属mod
    /// </summary>
    [ObservableProperty]
    private bool _loadMoreMod;
    /// <summary>
    /// 是否选择了项目
    /// </summary>
    [ObservableProperty]
    private bool _isSelect;
    /// <summary>
    /// 是否为标记模式
    /// </summary>
    [ObservableProperty]
    private bool _set;
    /// <summary>
    /// 搜索源数据是否加载了
    /// </summary>
    [ObservableProperty]
    private bool _sourceLoad;
    /// <summary>
    /// 是否为模组升级模式
    /// </summary>
    [ObservableProperty]
    private bool _isUpgrade;
    [ObservableProperty]
    private bool _emptyOptifineDisplay;
    [ObservableProperty]
    private bool _nextPage;
    [ObservableProperty]
    private bool _nextPageDisplay;

    /// <summary>
    /// 下载类型
    /// </summary>
    [ObservableProperty]
    private int _type = -1;
    /// <summary>
    /// 排序类型
    /// </summary>
    [ObservableProperty]
    private int _sortType = -1;
    /// <summary>
    /// 搜索源
    /// </summary>
    [ObservableProperty]
    private int _downloadSource = -1;
    /// <summary>
    /// 页数
    /// </summary>
    [ObservableProperty]
    private int? _page = 0;
    /// <summary>
    /// 分类
    /// </summary>
    [ObservableProperty]
    private int _categorie;
    /// <summary>
    /// 文件列表页数
    /// </summary>
    [ObservableProperty]
    private int? _pageDownload;
    [ObservableProperty]
    private int _maxPage;
    [ObservableProperty]
    private int _maxPageDownload;

    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersion;
    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string? _name;
    /// <summary>
    /// 高清修复游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersionOptifine;
    /// <summary>
    /// 文件列表游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersionDownload;
    [ObservableProperty]
    private string? _modDownloadText = App.Lang("AddWindow.Text7");

    /// <summary>
    /// 下载源列表
    /// </summary>
    private readonly List<SourceType> _sourceTypeList = [];
    /// <summary>
    /// 类型列表
    /// </summary>
    private readonly Dictionary<int, string> _categories = [];
    /// <summary>
    /// Mod下载项目显示列表
    /// </summary>
    private readonly List<FileModVersionModel> _modList = [];
    /// <summary>
    /// 高清修复列表
    /// </summary>
    private readonly List<OptifineVersionItemModel> _optifineList = [];
    /// <summary>
    /// 下载源列表
    /// </summary>
    private readonly List<string> _sourceTypeNameList =
    [
        SourceType.CurseForge.GetName(),
        SourceType.Modrinth.GetName(),
    ];

    /// <summary>
    /// 当前文件类型
    /// </summary>
    private FileType _now;
    /// <summary>
    /// 下载项目
    /// </summary>
    private FileItemModel? _last;
    /// <summary>
    /// 下载的模组项目
    /// </summary>
    private DownloadModArg? _modsave;
    /// <summary>
    /// 是否在加载
    /// </summary>
    private bool _load = false;
    /// <summary>
    /// 是否关闭
    /// </summary>
    private bool _close = false;

    /// <summary>
    /// 上一个下载源
    /// </summary>
    private SourceType _lastType = SourceType.McMod;
    /// <summary>
    /// 上一个下载ID
    /// </summary>
    private string? _lastId;

    /// <summary>
    /// 是否已经显示
    /// </summary>
    public bool Display { get; set; }

    private readonly string _useName;

    public AddControlModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {
        _useName = ToString() ?? "AddControlModel";
    }

    partial void OnIsUpgradeChanged(bool value)
    {
        ModDownloadText = App.Lang(value ? "AddWindow.Text15" : "AddWindow.Text7");
    }

    partial void OnLoadMoreModChanged(bool value)
    {
        ModsLoad(true);
    }

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

    partial void OnVersionDisplayChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                _lastId = null;
                VersionDisplay = false;
            });
        }
        else
        {
            Model.PopBack();
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
    /// 下载类型选择
    /// </summary>
    /// <param name="value"></param>
    async partial void OnTypeChanged(int value)
    {
        if (!Display)
            return;

        if (Type == 5)
        {
            await OptifineOpen();
            return;
        }

        _load = true;

        IsSelect = false;
        _now = (FileType)(Type + 1);
        GameVersionList.Clear();
        SortTypeList.Clear();
        CategorieList.Clear();

        Page = 0;

        FileList.Clear();
        DownloadSourceList.Clear();

        _sourceTypeList.Clear();
        _sourceTypeList.AddRange(WebBinding.GetSourceList(_now));
        _sourceTypeList.ForEach(item => DownloadSourceList.Add(item.GetName()));

        _load = false;

        DownloadSource = 0;
    }

    /// <summary>
    /// 排序类型选择
    /// </summary>
    /// <param name="value"></param>
    partial void OnSortTypeChanged(int value)
    {
        Refresh();
    }

    /// <summary>
    /// 分类选择
    /// </summary>
    /// <param name="value"></param>
    partial void OnCategorieChanged(int value)
    {
        Refresh();
    }

    /// <summary>
    /// 页数修改
    /// </summary>
    /// <param name="value"></param>
    async partial void OnPageChanged(int? value)
    {
        if (!Display || _load)
        {
            return;
        }

        await GetList();
    }

    /// <summary>
    /// 文件列表页数修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageDownloadChanged(int? value)
    {
        if (!Display || _load)
        {
            return;
        }

        LoadFile();
    }

    /// <summary>
    /// 搜索源修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnDownloadSourceChanged(int value)
    {
        LoadSourceData();
    }

    /// <summary>
    /// 游戏版本修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionChanged(string? value)
    {
        Refresh();
    }

    /// <summary>
    /// 文件列表游戏版本修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionDownloadChanged(string? value)
    {
        if (!Display || _load)
        {
            return;
        }

        //OptiFine
        if (Type == 5)
        {
            return;
        }

        LoadFile();
    }

    /// <summary>
    /// 获取项目列表
    /// </summary>
    [RelayCommand]
    public async Task GetList()
    {
        var type = _sourceTypeList[DownloadSource];

        Model.Progress(App.Lang("AddWindow.Info2"));
        if (type == SourceType.McMod)
        {
            //McMod搜索源
            var data = await WebBinding.SearchMcmod(Name ?? "", Page ?? 0, Obj.Loader, GameVersion ?? "", _categories[Categorie], SortType);
            if (data == null)
            {
                Model.ProgressClose();
                Model.Show(App.Lang("AddWindow.Error2"));
                return;
            }

            ClearList();

            foreach (var item in data)
            {
                item.Add = this;
                DisplayList.Add(item);
            }

            _last = null;

            EmptyDisplay = DisplayList.Count == 0;

            Model.ProgressClose();
        }
        else
        {
            //其他搜索源
            var res = await WebBinding.GetList(_now, type,
                GameVersion, Name, Page ?? 0,
                SortType, Categorie < 0 ? "" :
                    _categories[Categorie], Obj.Loader);

            var data = res.List;

            MaxPage = res.Count / 20;
            var page = 0;
            if (type == SourceType.Modrinth)
            {
                page = Page ?? 0;
            }

            NextPage = (MaxPage - Page) > 0;

            if (data == null)
            {
                Model.ProgressClose();
                Model.Show(App.Lang("AddWindow.Error2"));
                return;
            }

            ClearList();

            if (_now == FileType.Mod)
            {
                int b = 0;
                for (int a = page * 50; a < data.Count; a++, b++)
                {
                    if (b >= 50)
                    {
                        break;
                    }
                    var item = data[a];
                    item.Add = this;
                    if (Obj.Mods.ContainsKey(item.ID))
                    {
                        item.IsDownload = true;
                    }
                    DisplayList.Add(item);
                }
            }
            else
            {
                int b = 0;
                for (int a = page * 50; a < data.Count; a++, b++)
                {
                    if (b >= 50)
                    {
                        break;
                    }
                    var item = data[a];
                    item.Add = this;
                    DisplayList.Add(item);
                }
            }

            _last = null;

            EmptyDisplay = DisplayList.Count == 0;

            Model.ProgressClose();
        }

        OnPropertyChanged("ScrollToHome");

        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    /// <summary>
    /// 根据名字刷新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GetNameList()
    {
        if (!string.IsNullOrWhiteSpace(Name) && Page != 0)
        {
            Page = 0;
            return;
        }

        await GetList();
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GoFile()
    {
        var item = File;
        if (item == null)
        {
            return;
        }

        var res = await Model.ShowWait(
            string.Format(Set ? App.Lang("AddWindow.Info8") : App.Lang("AddWindow.Info1"),
            item.Name));
        if (res)
        {
            Install(item);
        }
    }

    /// <summary>
    /// 刷新
    /// </summary>
    [RelayCommand]
    public void Refresh1()
    {
        LoadFile();
    }

    /// <summary>
    /// 转到文件列表
    /// </summary>
    [RelayCommand]
    public void GoInstall()
    {
        if (_last == null)
        {
            Model.Show(App.Lang("AddWindow.Error1"));
            return;
        }

        Install();
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
        var list = await WebBinding.GetOptifine();
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
    /// 下载模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadMod()
    {
        Model.Progress(App.Lang("AddWindow.Info5"));
        bool res;
        if (DownloadModList.Any(item => item is ModUpgradeModel))
        {
            var list = DownloadModList.Where(item => item is ModUpgradeModel && item.Download)
                .Select(item => new DownloadModArg()
                {
                    Info = item.Items[item.SelectVersion].Info,
                    Item = item.Items[item.SelectVersion].Item,
                    Old = (item as ModUpgradeModel)!.Obj
                }).ToList();

            res = await WebBinding.DownloadMod(Obj, list);
        }
        else
        {
            var list = DownloadModList.Where(item => item.Download)
                                .Select(item => item.Items[item.SelectVersion]).ToList();
            if (_modsave != null)
            {
                list.Add(_modsave);
            }

            res = await WebBinding.DownloadMod(Obj, list);
        }
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("AddWindow.Error5"));
            if (_last != null)
            {
                _last.IsDownload = false;
                _last.NowDownload = false;
            }
        }
        else
        {
            if (_last != null)
            {
                _last.NowDownload = false;
                _last.IsDownload = true;
            }
        }
        CloseModDownloadDisplay();
    }

    /// <summary>
    /// 选择下载所有模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadAllMod()
    {
        foreach (var item in DownloadModList)
        {
            item.Download = true;
        }
        await DownloadMod();
    }

    /// <summary>
    /// 下一页
    /// </summary>
    [RelayCommand]
    public void Next()
    {
        if (_load || IsDownload)
        {
            return;
        }

        Page += 1;
    }

    [RelayCommand]
    public void NextVersion()
    {
        if (_load)
        {
            return;
        }

        PageDownload++;
    }

    public async void LoadSourceData()
    {
        if (!Display || _load)
        {
            return;
        }

        SourceLoad = false;
        _load = true;
        GameVersionList.Clear();
        SortTypeList.Clear();
        CategorieList.Clear();

        ClearList();
        var type = _sourceTypeList[DownloadSource];

        //CF搜索源
        if (type == SourceType.CurseForge)
        {
            SortTypeList.AddRange(LanguageBinding.GetCurseForgeSortTypes());

            Model.Progress(App.Lang("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            var list1 = await GameBinding.GetCurseForgeCategories(_now);
            Model.ProgressClose();
            if (list == null || list1 == null)
            {
                _load = false;
                LoadFail();
                return;
            }

            _categories.Clear();
            _categories.Add(0, "");
            int a = 1;
            foreach (var item in list1)
            {
                _categories.Add(a++, item.Key);
            }

            var list2 = new List<string>() { "" };

            list2.AddRange(list1.Values);

            GameVersionList.AddRange(list);
            CategorieList.AddRange(list2);

            if (GameVersionList.Contains(Obj.Version))
            {
                GameVersionOptifine = GameVersionDownload = GameVersion = Obj.Version;
            }
            else
            {
                GameVersionOptifine = GameVersionDownload = GameVersion = GameVersionList.FirstOrDefault();
            }

            SortType = 1;
            Categorie = 0;

            await GetList();
        }
        //MO搜索源
        else if (type == SourceType.Modrinth)
        {
            SortTypeList.AddRange(LanguageBinding.GetModrinthSortTypes());

            Model.Progress(App.Lang("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories(_now);
            Model.ProgressClose();
            if (list == null || list1 == null)
            {
                _load = false;
                LoadFail();
                return;
            }
            GameVersionList.AddRange(list);

            _categories.Clear();
            _categories.Add(0, "");
            int a = 1;
            foreach (var item in list1)
            {
                _categories.Add(a++, item.Key);
            }

            var list2 = new List<string>() { "" };

            list2.AddRange(list1.Values);

            GameVersionList.AddRange(list);
            CategorieList.AddRange(list2);

            if (GameVersionList.Contains(Obj.Version))
            {
                GameVersionDownload = GameVersionOptifine = GameVersion = Obj.Version;
            }
            else
            {
                GameVersionDownload = GameVersionOptifine = GameVersion = GameVersionList.FirstOrDefault();
            }

            SortType = 0;
            Categorie = 0;

            await GetList();
        }
        //McMod搜索源
        else if (type == SourceType.McMod)
        {
            Model.Progress(App.Lang("AddModPackWindow.Info4"));
            var list = await GameBinding.GetMcModCategories();
            Model.ProgressClose();
            if (list == null)
            {
                _load = false;
                LoadFail();
                return;
            }
            GameVersionList.AddRange(list.Versions);

            _categories.Clear();
            _categories.Add(0, "");
            int a = 1;
            foreach (var item in list.Types)
            {
                _categories.Add(a++, item);
            }

            SortTypeList.Add("");
            SortTypeList.AddRange(list.Sorts);

            CategorieList.AddRange(_categories.Values);

            if (GameVersionList.Contains(Obj.Version))
            {
                GameVersionDownload = GameVersionOptifine = GameVersion = Obj.Version;
            }
            else
            {
                GameVersionDownload = GameVersionOptifine = GameVersion = GameVersionList.FirstOrDefault();
            }

            await GetList();
        }

        SourceLoad = true;
        _load = false;
    }

    /// <summary>
    /// 加载模组列表
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
    /// 选择项目
    /// </summary>
    /// <param name="last"></param>
    public void SetSelect(FileItemModel last)
    {
        if (IsDownload)
            return;

        IsSelect = true;
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = last;
        _last.IsSelect = true;
    }

    /// <summary>
    /// 转到下载类型
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="pid">项目ID</param>
    public async void GoFile(SourceType type, string pid)
    {
        Type = (int)FileType.Mod - 1;
        DownloadSource = (int)type;
        await Task.Run(() =>
        {
            while ((!Display || _load) && !_close)
            {
                Thread.Sleep(100);
            }
        });

        _lastId = pid;

        _load = true;
        PageDownload = 0;
        LoadFile();
        _load = false;
    }

    /// <summary>
    /// 打开文件列表
    /// </summary>
    public void Install()
    {
        if (IsDownload)
        {
            Model.Show(App.Lang("AddWindow.Info9"));
            return;
        }

        _lastType = SourceType.McMod;
        _lastId = null;

        _load = true;
        PageDownload = 0;
        LoadFile();
        _load = false;
    }

    /// <summary>
    /// 开始下载文件
    /// </summary>
    /// <param name="data"></param>
    public async void Install(FileVersionItemModel data)
    {
        var type = _sourceTypeList[DownloadSource];
        if (Set)
        {
            if (type == SourceType.CurseForge)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as CurseForgeModObj.DataObj);
            }
            else if (type == SourceType.Modrinth)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as ModrinthVersionObj);
            }
            return;
        }

        ModInfoObj? mod = null;
        if (_now == FileType.Mod && Obj.Mods.TryGetValue(data.ID, out mod))
        {
            var res1 = await Model.ShowWait(App.Lang("AddWindow.Info15"));
            if (!res1)
            {
                return;
            }
        }

        var last = _last!;
        IsDownload = true;
        if (last != null)
        {
            last.NowDownload = true;
        }

        VersionDisplay = false;
        bool res = false;

        //数据包
        if (_now == FileType.DataPacks)
        {
            //选择存档
            var list = await GameBinding.GetWorlds(Obj);
            if (list.Count == 0)
            {
                Model.Show(App.Lang("AddWindow.Error6"));
                IsDownload = false;
                return;
            }

            var world = new List<string>();
            list.ForEach(item => world.Add(item.LevelName));
            var res1 = await Model.ShowCombo(App.Lang("AddWindow.Info7"), world);
            if (res1.Cancel)
            {
                IsDownload = false;
                return;
            }
            var item = list[res1.Index];

            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await WebBinding.Download(item,
                        data.Data as CurseForgeModObj.DataObj),
                    SourceType.Modrinth => await WebBinding.Download(item,
                        data.Data as ModrinthVersionObj),
                    _ => false
                };
                IsDownload = false;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("AddWindow.Error7"), e);
                res = false;
            }
        }
        //模组
        else if (_now == FileType.Mod)
        {
            try
            {
                var list = (type == SourceType.McMod ? _lastType : type) switch
                {
                    SourceType.CurseForge => await WebBinding.GetDownloadModList(Obj,
                    data.Data as CurseForgeModObj.DataObj),
                    SourceType.Modrinth => await WebBinding.GetDownloadModList(Obj,
                    data.Data as ModrinthVersionObj),
                    _ => null
                };
                if (list == null)
                {
                    Model.Show(App.Lang("AddWindow.Error9"));
                    IsDownload = false;
                    return;
                }

                if (list.List!.Count == 0)
                {
                    res = await WebBinding.DownloadMod(Obj, new List<DownloadModArg>()
                    {
                        new()
                        {
                            Item = list.Item!,
                            Info = list.Info!,
                            Old = await Obj.ReadMod(mod)
                        }
                    });
                    IsDownload = false;
                }
                else
                {
                    //添加模组信息
                    _modList.Clear();
                    _modList.AddRange(list.List);
                    _modsave = new()
                    {
                        Item = list.Item!,
                        Info = list.Info!,
                        Old = await Obj.ReadMod(mod)
                    };
                    OpenModDownloadDisplay();
                    _modList.ForEach(item =>
                    {
                        if (item.Optional == false)
                        {
                            item.Download = true;
                        }
                    });
                    ModsLoad();
                    return;
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("AddWindow.Error8"), e);
                res = false;
            }
        }
        else
        {
            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await WebBinding.Download(_now, Obj,
                        data.Data as CurseForgeModObj.DataObj),
                    SourceType.Modrinth => await WebBinding.Download(_now, Obj,
                        data.Data as ModrinthVersionObj),
                    _ => false
                };
                IsDownload = false;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("AddWindow.Error8"), e);
                res = false;
            }
        }
        if (res)
        {
            Model.Notify(App.Lang("Text.Downloaded"));
            if (last != null)
            {
                last.NowDownload = false;
                last.IsDownload = true;
            }
        }
        else
        {
            if (last != null)
            {
                last.NowDownload = false;
            }
            Model.Show(App.Lang("AddWindow.Error5"));
        }
    }

    /// <summary>
    /// 刷新列表
    /// </summary>
    public async void Refresh()
    {
        if (!Display || _load)
        {
            return;
        }

        await GetList();
    }

    /// <summary>
    /// 加载失败
    /// </summary>
    private async void LoadFail()
    {
        var res = await Model.ShowWait(App.Lang("AddModPackWindow.Error4"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (DownloadSource < _sourceTypeList.Count)
        {
            res = await Model.ShowWait(App.Lang("AddModPackWindow.Info5"));
            if (res)
            {
                DownloadSource++;
            }
        }
    }

    /// <summary>
    /// 加载文件列表
    /// </summary>
    /// <param name="id">项目ID</param>
    private async void LoadFile()
    {
        FileList.Clear();

        var type = _sourceTypeList[DownloadSource];
        if (type == SourceType.McMod)
        {
            if (_lastType == SourceType.McMod)
            {
                var obj1 = (_last!.Data as McModSearchItemObj)!;
                if (obj1.CurseforgeId != null && obj1.ModrinthId != null)
                {
                    var res = await Model.ShowCombo(App.Lang("AddWindow.Info14"), _sourceTypeNameList);
                    if (res.Cancel)
                    {
                        return;
                    }
                    _lastType = type = res.Index == 0 ? SourceType.CurseForge : SourceType.Modrinth;
                    _lastId = type == SourceType.CurseForge ? obj1.CurseforgeId : obj1.ModrinthId;
                }
                else if (obj1.CurseforgeId != null)
                {
                    _lastId = obj1.CurseforgeId;
                    _lastType = type = SourceType.CurseForge;
                }
                else if (obj1.ModrinthId != null)
                {
                    _lastId = obj1.ModrinthId;
                    _lastType = type = SourceType.Modrinth;
                }
            }
            else
            {
                type = _lastType;
            }
        }

        if (type == SourceType.McMod)
        {
            Model.Show(App.Lang("AddWindow.Error11"));
            return;
        }

        VersionDisplay = true;
        var page = 0;
        List<FileVersionItemModel>? list = null;
        Model.Progress(App.Lang("AddWindow.Info3"));
        if (type == SourceType.CurseForge)
        {
            var res = await WebBinding.GetFileList(type, _lastId ??
                (_last!.Data as CurseForgeObjList.DataObj)!.Id.ToString(), PageDownload ?? 0,
                GameVersionDownload, Obj.Loader, _now);
            list = res.List;
            MaxPageDownload = res.Count / 50;
        }
        else if (type == SourceType.Modrinth)
        {
            var res = await WebBinding.GetFileList(type, _lastId ??
                (_last!.Data as ModrinthSearchObj.HitObj)!.ProjectId, 0,
                GameVersionDownload, _now == FileType.Mod ? Obj.Loader : Loaders.Normal, _now);
            list = res.List;
            MaxPageDownload = res.Count / 50;
            page = PageDownload ?? 0;
        }

        NextPageDisplay = (MaxPageDownload - PageDownload) > 0;

        if (list == null)
        {
            Model.Show(App.Lang("AddWindow.Error3"));
            Model.ProgressClose();
            return;
        }

        if (_now == FileType.Mod)
        {
            int b = 0;
            for (int a = page * 50; a < list.Count; a++, b++)
            {
                if (b >= 50)
                {
                    break;
                }
                var item = list[a];
                item.Add = this;
                if (Obj.Mods.TryGetValue(item.ID, out var value)
                    && value.FileId == item.ID1)
                {
                    item.IsDownload = true;
                }
                FileList.Add(item);
            }
        }
        else
        {
            int b = 0;
            for (int a = page * 50; a < list.Count; a++, b++)
            {
                if (b >= 50)
                {
                    break;
                }
                var item = list[a];
                item.Add = this;
                FileList.Add(item);
            }
        }

        EmptyVersionDisplay = FileList.Count == 0;

        Model.ProgressClose();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    /// <summary>
    /// 打开高清修复列表
    /// </summary>
    public async Task OptifineOpen()
    {
        OptifineDisplay = true;
        await LoadOptifineList();
    }

    /// <summary>
    /// 转到文件类型
    /// </summary>
    /// <param name="file">文件类型</param>
    public async void GoTo(FileType file)
    {
        if (file == FileType.Optifne)
        {
            await OptifineOpen();
            if (GameVersionList.Contains(Obj.Version))
            {
                GameVersionOptifine = Obj.Version;
            }
        }
        else
        {
            Type = (int)file - 1;
            DownloadSource = 0;
        }
    }

    /// <summary>
    /// 上一页
    /// </summary>
    public void Back()
    {
        if (_load || IsDownload || Page <= 0)
        {
            return;
        }

        Page -= 1;
    }

    /// <summary>
    /// 上一页
    /// </summary>
    public void BackVersion()
    {
        if (_load || PageDownload <= 0)
        {
            return;
        }

        Page -= 1;
    }

    /// <summary>
    /// 重载
    /// </summary>
    public void Reload()
    {
        if (VersionDisplay)
        {
            Refresh1();
        }
        else
        {
            Refresh();
        }
    }

    /// <summary>
    /// 转到标记
    /// </summary>
    public async Task GoSet()
    {
        Set = true;

        Type = (int)FileType.Mod - 1;
        DownloadSource = 0;
        await Task.Run(() =>
        {
            while (!_close)
                Thread.Sleep(100);
        });
    }

    /// <summary>
    /// 打开文件列表
    /// </summary>
    /// <param name="item">项目</param>
    public void Install(FileItemModel item)
    {
        SetSelect(item);
        Install();
    }

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
        _modsave = null;
        _modList.ForEach(item =>
        {
            item.Download = true;
        });
        ModsLoad();
    }

    private void OpenModDownloadDisplay()
    {
        ModDownloadDisplay = true;
        Model.PushBack(back: CloseModDownloadDisplay);
    }

    private void CloseModDownloadDisplay()
    {
        if (IsUpgrade)
        {
            WindowClose();
        }
        ModDownloadDisplay = false;
        Model.PopBack();
        if (_last != null)
        {
            _last.NowDownload = false;
        }
        DownloadModList.Clear();
        IsDownload = false;
    }

    private void ClearList()
    {
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
    }

    public void SetSelect(OptifineVersionItemModel item)
    {
        if (OptifineItem != null)
        {
            OptifineItem.IsSelect = false;
        }
        OptifineItem = item;
        item.IsSelect = true;
    }

    public async void Install(OptifineVersionItemModel item)
    {
        var res = await Model.ShowWait(string.Format(
            App.Lang("AddWindow.Info10"), item.Version));
        if (!res)
        {
            return;
        }
        Model.Progress(App.Lang("AddWindow.Info11"));
        var res1 = await WebBinding.DownloadOptifine(Obj, item.Obj);
        Model.ProgressClose();
        if (res1.State == false)
        {
            Model.Show(res1.Message!);
        }
        else
        {
            Model.Notify(App.Lang("Text.Downloaded"));
            OptifineDisplay = false;
        }
    }

    public override void Close()
    {
        if (VersionDisplay || OptifineDisplay || ModDownloadDisplay)
        {
            Model.PopBack();
        }

        _close = true;
        _load = true;
        Model.RemoveChoiseData(_useName);
        _modList.Clear();
        _optifineList.Clear();
        DownloadOptifineList.Clear();
        DownloadModList.Clear();
        FileList.Clear();
        ClearList();
    }
}
