using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddControlModel : GameModel, IAddWindow
{
    /// <summary>
    /// 显示的高清修复列表
    /// </summary>
    public ObservableCollection<OptifineObj> DownloadOptifineList { get; init; } = new();
    /// <summary>
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<DownloadModModel> DownloadModList { get; init; } = new();
    /// 显示的<summary>
    /// 下载类型列表
    /// </summary>
    public List<string> TypeList { get; init; } = LanguageBinding.GetAddType();
    /// <summary>
    /// 显示的游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    /// <summary>
    /// 显示的文件列表
    /// </summary>
    public ObservableCollection<FileDisplayObj> FileList { get; init; } = new();
    /// <summary>
    /// 显示的项目列表
    /// </summary>
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = new();
    /// <summary>
    /// 显示的下载源列表
    /// </summary>
    public ObservableCollection<string> DownloadSourceList { get; init; } = new();
    /// <summary>
    /// 显示的排序列表
    /// </summary>
    public ObservableCollection<string> SortTypeList { get; init; } = new();
    /// <summary>
    /// 显示的分类列表
    /// </summary>
    public ObservableCollection<string> CategorieList { get; init; } = new();

    /// <summary>
    /// 下载源列表
    /// </summary>
    public readonly List<SourceType> SourceTypeList = new();
    /// <summary>
    /// 类型列表
    /// </summary>
    public readonly Dictionary<int, string> Categories = new();
    /// <summary>
    /// Mod下载项目显示列表
    /// </summary>
    public readonly List<DownloadModModel> ModList = new();
    /// <summary>
    /// 高清修复列表
    /// </summary>
    public readonly List<OptifineObj> OptifineList = new();
    /// <summary>
    /// 下载源列表
    /// </summary>
    private readonly List<string> SourceTypeNameList = new()
    {
        SourceType.CurseForge.GetName(),
        SourceType.Modrinth.GetName(),
    };

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
    private (DownloadItemObj, ModInfoObj) _modsave;
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
    private string _lastId;

    /// <summary>
    /// 是否已经显示
    /// </summary>
    public bool Display { get; set; }

    /// <summary>
    /// 高清修复项目
    /// </summary>
    [ObservableProperty]
    private OptifineObj? _optifineItem;
    /// <summary>
    /// 项目
    /// </summary>
    [ObservableProperty]
    private FileDisplayObj? _file;
    /// <summary>
    /// 下载的模组
    /// </summary>
    [ObservableProperty]
    private DownloadModModel? _mod;

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
    /// 启用翻页
    /// </summary>
    [ObservableProperty]
    private bool _enablePage;
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
    private int _pageDownload;

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

    /// <summary>
    /// 展示搜索选项
    /// </summary>
    [ObservableProperty]
    private bool _displayFilter = true;

    public AddControlModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {

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

        _now = (FileType)(Type + 1);
        GameVersionList.Clear();
        SortTypeList.Clear();
        CategorieList.Clear();

        Page = 0;

        FileList.Clear();
        DownloadSourceList.Clear();

        SourceTypeList.Clear();
        SourceTypeList.AddRange(WebBinding.GetSourceList(_now));
        SourceTypeList.ForEach(item => DownloadSourceList.Add(item.GetName()));

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
    partial void OnPageDownloadChanged(int value)
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
    async partial void OnDownloadSourceChanged(int value)
    {
        if (!Display || _load)
        {
            return;
        }

        _load = true;

        GameVersionList.Clear();
        SortTypeList.Clear();
        CategorieList.Clear();

        DisplayList.Clear();
        var type = SourceTypeList[DownloadSource];
        //CF搜索源
        if (type == SourceType.CurseForge)
        {
            SortTypeList.AddRange(LanguageBinding.GetCurseForgeSortTypes());

            Model.Progress(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            var list1 = await GameBinding.GetCurseForgeCategories(_now);
            Model.ProgressClose();
            if (list == null || list1 == null)
            {
                Model.ShowOk(App.GetLanguage("AddModPackWindow.Error4"), LoadFail);
                return;
            }

            Categories.Clear();
            Categories.Add(0, "");
            int a = 1;
            foreach (var item in list1)
            {
                Categories.Add(a++, item.Key);
            }

            var list2 = new List<string>()
            {
                ""
            };

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

            Model.Progress(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories(_now);
            Model.ProgressClose();
            if (list == null || list1 == null)
            {
                Model.ShowOk(App.GetLanguage("AddModPackWindow.Error4"), LoadFail);
                return;
            }
            GameVersionList.AddRange(list);

            Categories.Clear();
            Categories.Add(0, "");
            int a = 1;
            foreach (var item in list1)
            {
                Categories.Add(a++, item.Key);
            }

            var list2 = new List<string>()
            {
                ""
            };

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
            GameVersionList.Add("");
            GameVersionList.Add(Obj.Version);

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

        _load = false;
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
    /// 切换过滤显示
    /// </summary>
    [RelayCommand]
    public void ShowFilter()
    {
        DisplayFilter = !DisplayFilter;
    }

    /// <summary>
    /// 获取项目列表
    /// </summary>
    [RelayCommand]
    public async Task GetList()
    {
        var type = SourceTypeList[DownloadSource];
        Model.Progress(App.GetLanguage("AddWindow.Info2"));
        if (type == SourceType.McMod)
        {
            //McMod搜索源
            var data = await WebBinding.SearchMcmod(Name ?? "", Page ?? 0);
            if (data == null)
            {
                Model.ProgressClose();
                Model.Show(App.GetLanguage("AddWindow.Error2"));
                return;
            }

            DisplayList.Clear();

            foreach (var item in data)
            {
                DisplayList.Add(new(item, this));
            }

            OnPropertyChanged(nameof(DisplayList));

            _last = null;

            EmptyDisplay = DisplayList.Count == 0;

            Model.ProgressClose();
        }
        else
        {
            //其他搜索源
            var data = await WebBinding.GetList(_now, type,
                GameVersion, Name, Page ?? 0,
                SortType, Categorie < 0 ? "" :
                    Categories[Categorie], Obj.Loader);

            if (data == null)
            {
                Model.ProgressClose();
                Model.Show(App.GetLanguage("AddWindow.Error2"));
                return;
            }

            DisplayList.Clear();

            if (_now == FileType.Mod)
            {
                foreach (var item in data)
                {
                    if (Obj.Mods.ContainsKey(item.ID))
                    {
                        item.IsDownload = true;
                    }
                    DisplayList.Add(new(item, this));
                }
            }
            else
            {
                foreach (var item in data)
                {
                    DisplayList.Add(new(item, this));
                }
            }

            OnPropertyChanged(nameof(DisplayList));

            _last = null;

            EmptyDisplay = DisplayList.Count == 0;

            Model.ProgressClose();
        }
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
    /// 文件列表关闭
    /// </summary>
    [RelayCommand]
    public void VersionClose()
    {
        VersionDisplay = false;
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
            string.Format(Set ? App.GetLanguage("AddWindow.Info8") : App.GetLanguage("AddWindow.Info1"),
            item.Name));
        if (res)
        {
            Install1(item);
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
            Model.Show(App.GetLanguage("AddWindow.Error1"));
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
        GameVersionList.Clear();
        OptifineList.Clear();
        DownloadOptifineList.Clear();
        Model.Progress(App.GetLanguage("AddWindow.Info13"));
        var list = await WebBinding.GetOptifine();
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(App.GetLanguage("AddWindow.Error10"));
            return;
        }

        OptifineList.AddRange(list);

        GameVersionList.Add("");
        GameVersionList.AddRange(from item2 in list
                                 group item2 by item2.MCVersion into newgroup
                                 select newgroup.Key);

        LoadOptifineVersion();
    }

    /// <summary>
    /// 关闭高清修复列表
    /// </summary>
    [RelayCommand]
    public void OptifineClose()
    {
        OptifineDisplay = false;

        Type = 0;
        DownloadSource = 0;
    }

    /// <summary>
    /// 下载模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadMod()
    {
        Model.Progress(App.GetLanguage("AddWindow.Info5"));
        var list = DownloadModList.Where(item => item.Download)
                        .Select(item => item.Items[item.SelectVersion]).ToList();
        list.Add(_modsave);
        bool res;
        res = await WebBinding.DownloadMod(Obj, list);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.GetLanguage("AddWindow.Error5"));
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
        IsDownload = false;
        ModDownloadDisplay = false;
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    [RelayCommand]
    public void ModsLoad()
    {
        DownloadModList.Clear();
        if (LoadMoreMod)
        {
            DownloadModList.AddRange(ModList);
        }
        else
        {
            ModList.ForEach(item =>
            {
                if (item.Optional)
                    return;
                DownloadModList.Add(item);
            });
            if (DownloadModList.Count == 0)
            {
                LoadMoreMod = true;
            }
        }
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
    /// 取消下载模组
    /// </summary>
    [RelayCommand]
    public void DownloadModCancel()
    {
        if (_last != null)
        {
            _last.NowDownload = false;
        }
        DownloadModList.Clear();
        IsDownload = false;
        ModDownloadDisplay = false;
    }

    /// <summary>
    /// 下载高清修复
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadOptifine()
    {
        if (OptifineItem == null)
            return;

        var res = await Model.ShowWait(string.Format(
            App.GetLanguage("AddWindow.Info10"), OptifineItem.Version));
        if (!res)
            return;
        Model.Progress(App.GetLanguage("AddWindow.Info11"));
        var res1 = await WebBinding.DownloadOptifine(Obj, OptifineItem);
        Model.ProgressClose();
        if (res1.Item1 == false)
        {
            Model.Show(res1.Item2!);
        }
        else
        {
            Model.Notify(App.GetLanguage("AddWindow.Info12"));
            OptifineClose();
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
                Thread.Sleep(100);
        });

        LoadFile(pid);
    }

    /// <summary>
    /// 打开文件列表
    /// </summary>
    public void Install()
    {
        if (IsDownload)
        {
            Model.Show(App.GetLanguage("AddWindow.Info9"));
            return;
        }

        _lastType = SourceType.McMod;
        _lastId = "";

        LoadFile();
    }

    /// <summary>
    /// 开始下载文件
    /// </summary>
    /// <param name="data"></param>
    public async void Install1(FileDisplayObj data)
    {
        var type = SourceTypeList[DownloadSource];
        if (Set)
        {
            if (type == SourceType.CurseForge)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as CurseForgeModObj.Data);
            }
            else if (type == SourceType.Modrinth)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as ModrinthVersionObj);
            }
            return;
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
            //选择地图
            var list = await GameBinding.GetWorlds(Obj);
            if (list.Count == 0)
            {
                Model.Show(App.GetLanguage("AddWindow.Error6"));
                return;
            }

            var world = new List<string>();
            list.ForEach(item => world.Add(item.LevelName));
            var res1 = await Model.ShowCombo(App.GetLanguage("AddWindow.Info7"), world);
            if (res1.Cancel)
                return;
            var item = list[res1.Index];

            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await WebBinding.Download(item,
                        data.Data as CurseForgeModObj.Data),
                    SourceType.Modrinth => await WebBinding.Download(item,
                        data.Data as ModrinthVersionObj),
                    _ => false
                };
                IsDownload = false;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("AddWindow.Error7"), e);
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
                    SourceType.CurseForge => await WebBinding.DownloadMod(Obj,
                    data.Data as CurseForgeModObj.Data),
                    SourceType.Modrinth => await WebBinding.DownloadMod(Obj,
                    data.Data as ModrinthVersionObj),
                    _ => (null, null, null)
                };
                if (list.Item1 == null)
                {
                    Model.Show(App.GetLanguage("AddWindow.Error9"));
                    return;
                }
                if (list.Item3!.Count == 0)
                {
                    res = await WebBinding.DownloadMod(Obj,
                        new List<(DownloadItemObj, ModInfoObj)>() { (list.Item1!, list.Item2!) });
                    IsDownload = false;
                }
                else
                {
                    //添加模组信息
                    ModList.Clear();
                    ModList.AddRange(list.Item3);
                    _modsave = (list.Item1!, list.Item2!);
                    ModDownloadDisplay = true;
                    ModList.ForEach(item =>
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
                Logs.Error(App.GetLanguage("AddWindow.Error8"), e);
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
                        data.Data as CurseForgeModObj.Data),
                    SourceType.Modrinth => await WebBinding.Download(_now, Obj,
                        data.Data as ModrinthVersionObj),
                    _ => false
                };
                IsDownload = false;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("AddWindow.Error8"), e);
                res = false;
            }
        }
        if (res)
        {
            Model.Notify(App.GetLanguage("AddWindow.Info6"));
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
            Model.Show(App.GetLanguage("AddWindow.Error5"));
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
    private void LoadFail()
    {
        if (DownloadSource >= SourceTypeList.Count)
        {
            WindowClose();
        }
        else
        {
            DownloadSource++;
        }
    }

    /// <summary>
    /// 加载文件列表
    /// </summary>
    /// <param name="id">项目ID</param>
    private async void LoadFile(string? id = null)
    {
        FileList.Clear();

        List<FileDisplayObj>? list = null;
        var type = SourceTypeList[DownloadSource];
        if (type == SourceType.McMod)
        {
            if (_lastType == SourceType.McMod)
            {
                var obj1 = (_last!.Data.Data as McModSearchItemObj)!;
                if (obj1.curseforge_id != null && obj1.modrinth_id != null)
                {
                    var res = await Model.ShowCombo(App.GetLanguage("AddWindow.Info14"), SourceTypeNameList);
                    if (res.Cancel)
                    {
                        return;
                    }
                    _lastType = type = res.Index == 0 ? SourceType.CurseForge : SourceType.Modrinth;
                    _lastId = id = type == SourceType.CurseForge ? obj1.curseforge_id : obj1.modrinth_id;
                }
                else if (obj1.curseforge_id != null)
                {
                    _lastId = id = obj1.curseforge_id;
                    _lastType = type = SourceType.CurseForge;
                }
                else if (obj1.modrinth_id != null)
                {
                    _lastId = id = obj1.modrinth_id;
                    _lastType = type = SourceType.Modrinth;
                }
            }
            else
            {
                type = _lastType;
                id = _lastId;
            }
        }

        if (type == SourceType.McMod)
        {
            Model.Show(App.GetLanguage("AddWindow.Error11"));
            return;
        }

        VersionDisplay = true;
        Model.Progress(App.GetLanguage("AddWindow.Info3"));
        if (type == SourceType.CurseForge)
        {
            EnablePage = true;
            list = await WebBinding.GetPackFile(type, id ??
                (_last!.Data.Data as CurseForgeObjList.Data)!.id.ToString(), PageDownload,
                GameVersionDownload, Obj.Loader, _now);
        }
        else if (type == SourceType.Modrinth)
        {
            EnablePage = false;
            list = await WebBinding.GetPackFile(type, id ??
                (_last!.Data.Data as ModrinthSearchObj.Hit)!.project_id, PageDownload,
                GameVersionDownload, _now == FileType.Mod ? Obj.Loader : Loaders.Normal, _now);
        }

        if (list == null)
        {
            Model.Show(App.GetLanguage("AddWindow.Error3"));
            Model.ProgressClose();
            return;
        }

        if (_now == FileType.Mod)
        {
            foreach (var item in list)
            {
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
            foreach (var item in list)
            {
                FileList.Add(item);
            }
        }

        Model.ProgressClose();
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
        if (IsDownload)
        {
            return;
        }

        if (Page <= 0)
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
        if (IsDownload)
        {
            return;
        }

        Page += 1;
    }

    /// <summary>
    /// 重载
    /// </summary>
    public void Reload()
    {
        if (EnablePage)
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

    /// <summary>
    /// 加载高清修复列表
    /// </summary>
    public void LoadOptifineVersion()
    {
        DownloadOptifineList.Clear();
        var item = GameVersionOptifine;
        if (string.IsNullOrWhiteSpace(item))
        {
            DownloadOptifineList.AddRange(OptifineList);
        }
        else
        {
            DownloadOptifineList.AddRange(from item1 in OptifineList
                                          where item1.MCVersion == item
                                          select item1);
        }
    }

    protected override void Close()
    {
        _close = true;
        _load = true;
        ModList.Clear();
        OptifineList.Clear();
        DownloadOptifineList.Clear();
        DownloadModList.Clear();
        FileList.Clear();
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
    }
}
