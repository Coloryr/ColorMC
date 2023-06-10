using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
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

public partial class AddControlModel : ObservableObject
{
    private readonly IUserControl Con;

    public readonly List<SourceType> SourceTypeList = new();
    public readonly Dictionary<int, string> Categories = new();
    public readonly List<DownloadModDisplayModel> ModList = new();
    public readonly List<OptifineDisplayObj> OptifineList = new();

    private FileType now;
    private FileItemModel? last;
    private (DownloadItemObj, ModInfoObj) modsave;
    private bool load = false;

    public GameSettingObj Obj { get; private set; }
    public bool display = false;

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
    private OptifineDisplayObj? optifineItem;
    [ObservableProperty]
    private FileDisplayObj? file;
    [ObservableProperty]
    private DownloadModDisplayModel? mod;

    [ObservableProperty]
    private bool isDownload;
    [ObservableProperty]
    private bool emptyDisplay = true;
    [ObservableProperty]
    private bool optifineDisplay;
    [ObservableProperty]
    private bool modDownloadDisplay;
    [ObservableProperty]
    private bool versionDisplay;
    [ObservableProperty]
    private bool loadMoreMod;
    [ObservableProperty]
    private bool enablePage;
    [ObservableProperty]
    private bool isSelect;
    [ObservableProperty]
    public bool set;

    [ObservableProperty]
    private int type = -1;
    [ObservableProperty]
    private int sortType = -1;
    [ObservableProperty]
    private int downloadSource = -1;
    [ObservableProperty]
    private int page;
    [ObservableProperty]
    private int categorie;
    [ObservableProperty]
    private int pageDownload;

    [ObservableProperty]
    private string? gameVersion;
    [ObservableProperty]
    private string? name;
    [ObservableProperty]
    private string? gameVersionOptifine;
    [ObservableProperty]
    private string? gameVersionDownload;

    public AddControlModel(IUserControl con, GameSettingObj obj)
    {
        Con = con;
        Obj = obj;
    }

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
    public async void GoFile()
    {
        var window = Con.Window;
        var item = File;
        if (item == null)
            return;

        var res = await window.OkInfo.ShowWait(
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
        var window = Con.Window;
        if (last == null)
        {
            window.OkInfo.Show(App.GetLanguage("AddWindow.Error1"));
            return;
        }

        Install();
    }

    [RelayCommand]
    public async void LoadOptifineList()
    {
        var window = Con.Window;
        GameVersionList.Clear();
        OptifineList.Clear();
        DownloadOptifineList.Clear();
        window.ProgressInfo.Show(App.GetLanguage("AddWindow.Info13"));
        var list = await WebBinding.GetOptifine();
        window.ProgressInfo.Close();
        if (list == null)
        {
            window.OkInfo.Show(App.GetLanguage("AddWindow.Error10"));
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
    public async void DownloadMod()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("AddWindow.Info5"));
        var list = DownloadModList.Where(item => item.Download)
                        .Select(item => item.Items[item.SelectVersion]).ToList();
        list.Add(modsave);
        bool res;
        res = await WebBinding.DownloadMod(Obj, list);
        window.ProgressInfo.Close();
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("AddWindow.Error5"));
            if (last != null)
            {
                last.IsDownload = false;
                last.NowDownload = true;
            }
        }
        else
        {
            if (last != null)
            {
                last.NowDownload = false;
                last.IsDownload = true;
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
    public void DownloadAllMod()
    {
        foreach (var item in DownloadModList)
        {
            item.Download = true;
        }
        DownloadMod();
    }

    [RelayCommand]
    public void DownloadModCancel()
    {
        if (last != null)
        {
            last.NowDownload = false;
        }
        DownloadModList.Clear();
        IsDownload = false;
        ModDownloadDisplay = false;
    }

    [RelayCommand]
    public async void DownloadOptifine()
    {
        if (OptifineItem == null)
            return;

        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(string.Format(
            App.GetLanguage("AddWindow.Info10"), OptifineItem.Version));
        if (!res)
            return;
        window.ProgressInfo.Show(App.GetLanguage("AddWindow.Info11"));
        var res1 = await WebBinding.DownloadOptifine(Obj, OptifineItem);
        window.ProgressInfo.Close();
        if (res1.Item1 == false)
        {
            window.OkInfo.Show(res1.Item2!);
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("AddWindow.Info12"));
            OptifineClose();
        }
    }
    ///////////////////////////////////////////////////
    partial void OnTypeChanged(int value)
    {
        if (!display)
            return;

        if (Type == 5)
        {
            OptifineOpen();
            return;
        }

        load = true;

        now = (FileType)(Type + 1);
        GameVersionList.Clear();
        SortTypeList.Clear();
        CategorieList.Clear();

        Page = 0;

        FileList.Clear();
        DownloadSourceList.Clear();

        SourceTypeList.Clear();
        SourceTypeList.AddRange(WebBinding.GetSourceList(now));
        SourceTypeList.ForEach(item => DownloadSourceList.Add(item.GetName()));

        load = false;

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
        if (!display || load)
            return;

        Load();
    }

    partial void OnPageDownloadChanged(int value)
    {
        if (!display || load)
            return;

        LoadFile();
    }

    async partial void OnDownloadSourceChanged(int value)
    {
        if (!display || load)
            return;

        var window = Con.Window;
        load = true;

        GameVersionList.Clear();
        SortTypeList.Clear();
        CategorieList.Clear();

        DisplayList.Clear();
        var type = SourceTypeList[DownloadSource];
        if (type == SourceType.CurseForge)
        {
            SortTypeList.AddRange(GameBinding.GetCurseForgeSortTypes());

            window.ProgressInfo.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            var list1 = await GameBinding.GetCurseForgeCategories(now);
            window.ProgressInfo.Close();
            if (list == null || list1 == null)
            {
                window.OkInfo.Show(App.GetLanguage("AddModPackWindow.Error4"));
                window.Close();
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
            SortTypeList.AddRange(GameBinding.GetModrinthSortTypes());

            window.ProgressInfo.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories(now);
            window.ProgressInfo.Close();
            if (list == null || list1 == null)
            {
                window.OkInfo.Show(App.GetLanguage("AddModPackWindow.Error4"));
                window.Close();
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

        load = false;
    }

    partial void OnGameVersionChanged(string? value)
    {
        Refresh();
    }

    partial void OnGameVersionDownloadChanged(string? value)
    {
        if (!display || load)
            return;

        LoadFile();
    }
    ///////////////////////////////////////////////////
    public void SetSelect(FileItemModel last)
    {
        if (IsDownload)
            return;

        IsSelect = true;
        if (this.last != null)
        {
            this.last.IsSelect = false;
        }
        this.last = last;
        this.last.IsSelect = true;
    }

    public async void GoFile(SourceType type, string pid)
    {
        Type = (int)FileType.Mod - 1;
        DownloadSource = (int)type;
        await Task.Run(() =>
        {
            while (!display || load)
                Thread.Sleep(1000);
        });

        VersionDisplay = true;
        LoadFile(pid);
    }

    public void Install()
    {
        if (IsDownload)
        {
            var window = Con.Window;
            window.OkInfo.Show(App.GetLanguage("AddWindow.Info9"));
            return;
        }

        VersionDisplay = true;
        LoadFile();
    }

    public async void Install1(FileDisplayObj data)
    {
        var window = Con.Window;
        var type = SourceTypeList[DownloadSource];
        if (Set)
        {
            if (type == SourceType.CurseForge)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as CurseForgeObjList.Data.LatestFiles);
            }
            else if (type == SourceType.Modrinth)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as ModrinthVersionObj);
            }
            window.Close();
            return;
        }

        var last = this.last!;
        IsDownload = true;
        last.NowDownload = true;
        VersionDisplay = false;
        bool res = false;

        if (now == FileType.DataPacks)
        {
            var list = await GameBinding.GetWorlds(Obj);
            if (list.Count == 0)
            {
                window.OkInfo.Show(App.GetLanguage("AddWindow.Error6"));
                return;
            }

            var world = new List<string>();
            list.ForEach(item => world.Add(item.World.LevelName));
            await window.ComboInfo.Show(App.GetLanguage("AddWindow.Info7"), world);
            if (window.ComboInfo.Cancel)
                return;
            var item = list[window.ComboInfo.Read().Item1];

            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await WebBinding.Download(item.World,
                    data.Data as CurseForgeObjList.Data.LatestFiles),
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
        else if (now == FileType.Mod)
        {
            try
            {
                var list = type switch
                {
                    SourceType.CurseForge => await WebBinding.DownloadMod(Obj,
                    data.Data as CurseForgeObjList.Data.LatestFiles),
                    SourceType.Modrinth => await WebBinding.DownloadMod(Obj,
                    data.Data as ModrinthVersionObj),
                    _ => (null, null, null)
                };
                if (list.Item1 == null)
                {
                    window.OkInfo.Show(App.GetLanguage("AddWindow.Error9"));
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
                    modsave = (list.Item1!, list.Item2!);
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
                    SourceType.CurseForge => await WebBinding.Download(now, Obj,
                    data.Data as CurseForgeObjList.Data.LatestFiles),
                    SourceType.Modrinth => await WebBinding.Download(now, Obj,
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
            window.NotifyInfo.Show(App.GetLanguage("AddWindow.Info6"));
            last.NowDownload = false;
            last.IsDownload = true;
        }
        else
        {
            last.NowDownload = false;
            window.OkInfo.Show(App.GetLanguage("AddWindow.Error5"));
        }
    }

    public void Refresh()
    {
        if (!display || load)
            return;

        Load();
    }

    public async void Load()
    {
        var window = Con.Window;
        var type = SourceTypeList[DownloadSource];
        window.ProgressInfo.Show(App.GetLanguage("AddWindow.Info2"));
        if (type == SourceType.McMod)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                window.ProgressInfo.Close();
                return;
            }
            
            var data = await WebBinding.SearchMcmod(now, Name, Page);
            if (data == null)
            {
                window.ProgressInfo.Close();
                window.OkInfo.Show(App.GetLanguage("AddWindow.Error2"));
                return;
            }

            DisplayList.Clear();

            foreach (var item in data)
            {
                DisplayList.Add(new(item));
            }

            OnPropertyChanged(nameof(DisplayList));

            last = null;

            EmptyDisplay = DisplayList.Count == 0;

            window.ProgressInfo.Close();
        }
        else
        {
            var data = await WebBinding.GetList(now, type,
                GameVersion, Name, Page,
                SortType, Categorie < 0 ? "" :
                    Categories[Categorie], Obj.Loader);

            if (data == null)
            {
                window.ProgressInfo.Close();
                window.OkInfo.Show(App.GetLanguage("AddWindow.Error2"));
                return;
            }

            DisplayList.Clear();

            if (now == FileType.Mod)
            {
                foreach (var item in data)
                {
                    if (Obj.Mods.ContainsKey(item.ID))
                    {
                        item.IsDownload = true;
                    }
                    DisplayList.Add(new(item));
                }
            }
            else
            {
                foreach (var item in data)
                {
                    DisplayList.Add(new(item));
                }
            }

            OnPropertyChanged(nameof(DisplayList));

            last = null;

            EmptyDisplay = DisplayList.Count == 0;

            window.ProgressInfo.Close();
        }
    }

    public async void LoadFile(string? id = null)
    {
        FileList.Clear();

        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("AddWindow.Info3"));
        List<FileDisplayObj>? list = null;
        var type = SourceTypeList[DownloadSource];
        if (type == SourceType.CurseForge)
        {
            EnablePage = true;
            list = await WebBinding.GetPackFile(type, id ??
                (last!.Data?.Data as CurseForgeObjList.Data)!.id.ToString(), PageDownload,
                GameVersionDownload, Obj.Loader, now);
        }
        else if (type == SourceType.Modrinth)
        {
            EnablePage = false;
            list = await WebBinding.GetPackFile(type, id ??
                (last!.Data?.Data as ModrinthSearchObj.Hit)!.project_id, PageDownload,
                GameVersionDownload, now == FileType.Mod ? Obj.Loader : Loaders.Normal, now);
        }
        if (list == null)
        {
            window.OkInfo.Show(App.GetLanguage("AddWindow.Error3"));
            window.ProgressInfo.Close();
            return;
        }

        if (now == FileType.Mod)
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

        window.ProgressInfo.Close();
    }

    public void OptifineOpen()
    {
        OptifineDisplay = true;
        LoadOptifineList();
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
}
