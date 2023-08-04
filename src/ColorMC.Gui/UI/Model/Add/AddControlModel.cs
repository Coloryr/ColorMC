using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddControlModel : BaseModel, IAddWindow
{
    public readonly List<SourceType> SourceTypeList = new();
    public readonly Dictionary<int, string> Categories = new();
    public readonly List<DownloadModDisplayModel> ModList = new();
    public readonly List<OptifineDisplayObj> OptifineList = new();

    private FileType _now;
    private FileItemModel? _last;
    private (DownloadItemObj, ModInfoObj) _modsave;
    private bool _load = false;

    public GameSettingObj Obj { get; private set; }
    public bool Display { get; set; }

    public ObservableCollection<OptifineDisplayObj> DownloadOptifineList { get; init; } = new();
    public ObservableCollection<DownloadModDisplayModel> DownloadModList { get; init; } = new();
    public List<string> TypeList => GameBinding.GetAddType();
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<FileDisplayObj> FileList { get; init; } = new();
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = new();
    public ObservableCollection<string> DownloadSourceList { get; init; } = new();
    public ObservableCollection<string> SortTypeList { get; init; } = new();
    public ObservableCollection<string> CategorieList { get; init; } = new();

    [ObservableProperty]
    private OptifineDisplayObj? _optifineItem;
    [ObservableProperty]
    private FileDisplayObj? _file;
    [ObservableProperty]
    private DownloadModDisplayModel? _mod;

    [ObservableProperty]
    private bool _isDownload;
    [ObservableProperty]
    private bool _emptyDisplay = true;
    [ObservableProperty]
    private bool _optifineDisplay;
    [ObservableProperty]
    private bool _modDownloadDisplay;
    [ObservableProperty]
    private bool _versionDisplay;
    [ObservableProperty]
    private bool _loadMoreMod;
    [ObservableProperty]
    private bool _enablePage;
    [ObservableProperty]
    private bool _isSelect;
    [ObservableProperty]
    private bool _set;

    [ObservableProperty]
    private int _type = -1;
    [ObservableProperty]
    private int _sortType = -1;
    [ObservableProperty]
    private int _downloadSource = -1;
    [ObservableProperty]
    private int _page;
    [ObservableProperty]
    private int _categorie;
    [ObservableProperty]
    private int _pageDownload;

    [ObservableProperty]
    private string? _gameVersion;
    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private string? _gameVersionOptifine;
    [ObservableProperty]
    private string? _gameVersionDownload;

    public AddControlModel(IUserControl con, GameSettingObj obj) : base(con)
    {
        Obj = obj;
    }
    partial void OnTypeChanged(int value)
    {
        if (!Display)
            return;

        if (Type == 5)
        {
            OptifineOpen();
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

    partial void OnSortTypeChanged(int value)
    {
        Refresh();
    }

    partial void OnCategorieChanged(int value)
    {
        Refresh();
    }

    partial void OnPageChanged(int value)
    {
        if (!Display || _load)
            return;

        Load();
    }

    partial void OnPageDownloadChanged(int value)
    {
        if (!Display || _load)
            return;

        LoadFile();
    }

    async partial void OnDownloadSourceChanged(int value)
    {
        if (!Display || _load)
            return;

        _load = true;

        GameVersionList.Clear();
        SortTypeList.Clear();
        CategorieList.Clear();

        DisplayList.Clear();
        var type = SourceTypeList[DownloadSource];
        if (type == SourceType.CurseForge)
        {
            SortTypeList.AddRange(LanguageUtils.GetCurseForgeSortTypes());

            Progress(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            if (list == null)
            {
                ShowOk(App.GetLanguage("AddModPackWindow.Error4"), Window.Close);
                return;
            }
            var list1 = await GameBinding.GetCurseForgeCategories(_now);
            ProgressClose();
            if (list1 == null)
            {
                ShowOk(App.GetLanguage("AddModPackWindow.Error4"), Window.Close);
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

            Load();
        }
        else if (type == SourceType.Modrinth)
        {
            SortTypeList.AddRange(LanguageUtils.GetModrinthSortTypes());

            Progress(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories(_now);
            ProgressClose();
            if (list == null || list1 == null)
            {
                ShowOk(App.GetLanguage("AddModPackWindow.Error4"), Window.Close);
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

            Load();
        }
        else if (type == SourceType.McMod)
        {
            Load();
        }

        _load = false;
    }

    partial void OnGameVersionChanged(string? value)
    {
        Refresh();
    }

    partial void OnGameVersionDownloadChanged(string? value)
    {
        if (!Display || _load)
            return;

        LoadFile();
    }
    ///////////////////////////////////////////////////
    [RelayCommand]
    public void GetList()
    {
        Load();
    }

    [RelayCommand]
    public void GetNameList()
    {
        if (!string.IsNullOrWhiteSpace(Name) && Page != 0)
        {
            Page = 0;
            return;
        }

        Load();
    }

    [RelayCommand]
    public void VersionClose()
    {
        VersionDisplay = false;
    }

    [RelayCommand]
    public async Task GoFile()
    {
        var item = File;
        if (item == null)
            return;

        var res = await ShowWait(
            string.Format(Set ? App.GetLanguage("AddWindow.Info8") : App.GetLanguage("AddWindow.Info1"),
            item.Name));
        if (res)
        {
            Install1(item);
        }
    }

    [RelayCommand]
    public void Refresh1()
    {
        LoadFile();
    }

    [RelayCommand]
    public void GoInstall()
    {
        if (_last == null)
        {
            Show(App.GetLanguage("AddWindow.Error1"));
            return;
        }

        Install();
    }

    [RelayCommand]
    public async Task LoadOptifineList()
    {
        GameVersionList.Clear();
        OptifineList.Clear();
        DownloadOptifineList.Clear();
        Progress(App.GetLanguage("AddWindow.Info13"));
        var list = await WebBinding.GetOptifine();
        ProgressClose();
        if (list == null)
        {
            Show(App.GetLanguage("AddWindow.Error10"));
            return;
        }

        OptifineList.AddRange(list);

        GameVersionList.Add("");
        GameVersionList.AddRange(from item2 in list
                                 group item2 by item2.MC into newgroup
                                 select newgroup.Key);

        DownloadOptifineList.Clear();
        var item = GameVersionOptifine;
        if (string.IsNullOrWhiteSpace(item))
        {
            DownloadOptifineList.AddRange(OptifineList);
        }
        else
        {
            DownloadOptifineList.AddRange(from item1 in OptifineList
                                          where item1.MC == item
                                          select item1);
        }
    }

    [RelayCommand]
    public void OptifineClose()
    {
        OptifineDisplay = false;

        Type = 0;
        DownloadSource = 0;
    }

    [RelayCommand]
    public async Task DownloadMod()
    {
        Progress(App.GetLanguage("AddWindow.Info5"));
        var list = DownloadModList.Where(item => item.Download)
                        .Select(item => item.Items[item.SelectVersion]).ToList();
        list.Add(_modsave);
        bool res;
        res = await WebBinding.DownloadMod(Obj, list);
        ProgressClose();
        if (!res)
        {
            Show(App.GetLanguage("AddWindow.Error5"));
            if (_last != null)
            {
                _last.IsDownload = false;
                _last.NowDownload = true;
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

    [RelayCommand]
    public async Task DownloadAllMod()
    {
        foreach (var item in DownloadModList)
        {
            item.Download = true;
        }
        await DownloadMod();
    }

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

    [RelayCommand]
    public async Task DownloadOptifine()
    {
        if (OptifineItem == null)
            return;

        var res = await ShowWait(string.Format(
            App.GetLanguage("AddWindow.Info10"), OptifineItem.Version));
        if (!res)
            return;
        Progress(App.GetLanguage("AddWindow.Info11"));
        var res1 = await WebBinding.DownloadOptifine(Obj, OptifineItem);
        ProgressClose();
        if (res1.Item1 == false)
        {
            Show(res1.Item2!);
        }
        else
        {
            Notify(App.GetLanguage("AddWindow.Info12"));
            OptifineClose();
        }
    }
    ///////////////////////////////////////////////////
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

    public async void GoFile(SourceType type, string pid)
    {
        Type = (int)FileType.Mod - 1;
        DownloadSource = (int)type;
        await Task.Run(() =>
        {
            while ((!Display || _load) && !App.IsClose)
                Thread.Sleep(100);
        });

        VersionDisplay = true;
        LoadFile(pid);
    }

    public void Install()
    {
        if (IsDownload)
        {
            Show(App.GetLanguage("AddWindow.Info9"));
            return;
        }

        VersionDisplay = true;
        LoadFile();
    }

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
            Window.Close();
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

        if (_now == FileType.DataPacks)
        {
            var list = await GameBinding.GetWorlds(Obj);
            if (list.Count == 0)
            {
                Show(App.GetLanguage("AddWindow.Error6"));
                return;
            }

            var world = new List<string>();
            list.ForEach(item => world.Add(item.World.LevelName));
            var res1 = await ShowCombo(App.GetLanguage("AddWindow.Info7"), world);
            if (res1.Cancel)
                return;
            var item = list[res1.Index];

            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await WebBinding.Download(item.World,
                    data.Data as CurseForgeModObj.Data),
                    SourceType.Modrinth => await WebBinding.Download(item.World,
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
        else if (_now == FileType.Mod)
        {
            try
            {
                var list = type switch
                {
                    SourceType.CurseForge => await WebBinding.DownloadMod(Obj,
                    data.Data as CurseForgeModObj.Data),
                    SourceType.Modrinth => await WebBinding.DownloadMod(Obj,
                    data.Data as ModrinthVersionObj),
                    _ => (null, null, null)
                };
                if (list.Item1 == null)
                {
                    Show(App.GetLanguage("AddWindow.Error9"));
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
            Notify(App.GetLanguage("AddWindow.Info6"));
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
            Show(App.GetLanguage("AddWindow.Error5"));
        }
    }

    public void Refresh()
    {
        if (!Display || _load)
            return;

        Load();
    }

    private async void Load()
    {
        var type = SourceTypeList[DownloadSource];
        Progress(App.GetLanguage("AddWindow.Info2"));
        if (type == SourceType.McMod)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ProgressClose();
                return;
            }

            var data = await WebBinding.SearchMcmod(_now, Name, Page);
            if (data == null)
            {
                ProgressClose();
                Show(App.GetLanguage("AddWindow.Error2"));
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

            ProgressClose();
        }
        else
        {
            var data = await WebBinding.GetList(_now, type,
                GameVersion, Name, Page,
                SortType, Categorie < 0 ? "" :
                    Categories[Categorie], Obj.Loader);

            if (data == null)
            {
                ProgressClose();
                Show(App.GetLanguage("AddWindow.Error2"));
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

            ProgressClose();
        }
    }

    private async void LoadFile(string? id = null)
    {
        FileList.Clear();

        Progress(App.GetLanguage("AddWindow.Info3"));
        List<FileDisplayObj>? list = null;
        var type = SourceTypeList[DownloadSource];
        if (type == SourceType.CurseForge)
        {
            EnablePage = true;
            list = await WebBinding.GetPackFile(type, id ??
                (_last!.Data?.Data as CurseForgeObjList.Data)!.id.ToString(), PageDownload,
                GameVersionDownload, Obj.Loader, _now);
        }
        else if (type == SourceType.Modrinth)
        {
            EnablePage = false;
            list = await WebBinding.GetPackFile(type, id ??
                (_last!.Data?.Data as ModrinthSearchObj.Hit)!.project_id, PageDownload,
                GameVersionDownload, _now == FileType.Mod ? Obj.Loader : Loaders.Normal, _now);
        }
        if (list == null)
        {
            Show(App.GetLanguage("AddWindow.Error3"));
            ProgressClose();
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

        ProgressClose();
    }

    public async void OptifineOpen()
    {
        OptifineDisplay = true;
        await LoadOptifineList();
    }

    public void GoTo(FileType file)
    {
        if (file == FileType.Optifne)
        {
            OptifineOpen();
        }
        else
        {
            Type = (int)file - 1;
            DownloadSource = 0;
        }
    }
    public void Back()
    {
        if (IsDownload)
            return;

        if (Page <= 0)
            return;

        Page -= 1;
    }

    public void Next()
    {
        if (IsDownload)
            return;

        Page += 1;
    }

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

    public async Task GoSet()
    {
        Set = true;

        Type = (int)FileType.Mod - 1;
        DownloadSource = 0;
        await Task.Run(() =>
        {
            while (Set && !App.IsClose)
                Thread.Sleep(100);
        });
    }

    public void Install(FileItemModel item)
    {
        SetSelect(item);
        Install();
    }
}
