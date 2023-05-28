using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddModPackModel : ObservableObject
{
    private readonly IUserControl Con;

    public List<string> SourceList => GameBinding.GetSourceList();
    public ObservableCollection<FileDisplayObj> FileList { get; init; } = new();
    public ObservableCollection<string> GameVersionList { get; init; } = new();
    public ObservableCollection<string> CategorieList { get; init; } = new();
    public ObservableCollection<string> SortTypeList { get; init; } = new();
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = new();

    private readonly Dictionary<int, string> Categories = new();
    private FileItemModel? Last;
    private bool load = false;

    [ObservableProperty]
    private FileDisplayObj item;

    [ObservableProperty]
    private int source = -1;
    [ObservableProperty]
    private int categorie;
    [ObservableProperty]
    private int sortType;
    [ObservableProperty]
    private int page;
    [ObservableProperty]
    private int page1;
    [ObservableProperty]
    private string? gameVersion;
    [ObservableProperty]
    private string? gameVersion1;
    [ObservableProperty]
    private string? text;
    [ObservableProperty]
    private bool textEnable = true;
    [ObservableProperty]
    private bool pageEnable = true;
    [ObservableProperty]
    private bool pageEnable1 = true;
    [ObservableProperty]
    private bool enable = true;
    [ObservableProperty]
    private bool enable1 = true;
    [ObservableProperty]
    private bool isSelect = false;
    [ObservableProperty]
    private bool display = false;
    [ObservableProperty]
    private bool emptyDisplay = true;

    public AddModPackModel(IUserControl con)
    {
        Con = con;
    }

    partial void OnGameVersion1Changed(string? value)
    {
        Load1();
    }

    partial void OnPage1Changed(int value)
    {
        Load1();
    }

    partial void OnCategorieChanged(int value)
    {
        if (load)
            return;

        Load();
    }

    partial void OnSortTypeChanged(int value)
    {
        if (load)
            return;

        Load();
    }

    partial void OnGameVersionChanged(string? value)
    {
        if (load)
            return;

        Load();
    }

    partial void OnPageChanged(int value)
    {
        if (load)
            return;

        Load();
    }

    async partial void OnSourceChanged(int value)
    {
        load = true;

        Lock();

        CategorieList.Clear();
        SortTypeList.Clear();

        GameVersionList.Clear();
        Categories.Clear();

        DisplayList.Clear();
        OnPropertyChanged(nameof(DisplayList));

        var window = Con.Window;
        switch (Source)
        {
            case 0:
            case 1:
                SortTypeList.AddRange(Source == 0 ?
                    GameBinding.GetCurseForgeSortTypes() :
                    GameBinding.GetModrinthSortTypes());

                window.ProgressInfo.Show(App.GetLanguage("AddModPackWindow.Info4"));
                var list = Source == 0 ?
                    await GameBinding.GetCurseForgeGameVersions() :
                    await GameBinding.GetModrinthGameVersions();
                var list1 = Source == 0 ?
                    await GameBinding.GetCurseForgeCategories() :
                    await GameBinding.GetModrinthCategories();
                window.ProgressInfo.Close();
                if (list == null || list1 == null)
                {
                    window.OkInfo.Show(App.GetLanguage("AddModPackWindow.Error4"));
                    window.Close();
                    return;
                }
                GameVersionList.AddRange(list);

                Categories.Add(0, "");
                var a = 1;
                foreach (var item in list1)
                {
                    Categories.Add(a++, item.Key);
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

        load = false;
    }

    [RelayCommand]
    public void Select()
    {
        if (Last == null)
        {
            var window = Con.Window;
            window.OkInfo.Show(App.GetLanguage("AddModPackWindow.Error1"));
            return;
        }

        Install();
    }

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

    [RelayCommand]
    public void Cancel()
    {
        Display = false;
    }

    [RelayCommand]
    public void Search()
    {
        Load1();
    }

    [RelayCommand]
    public async void Download()
    {
        if (Item == null)
            return;

        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("AddModPackWindow.Info1"), Item.Name));
        if (res)
        {
            Install1(Item);
        }
    }

    public void SetSelect(FileItemModel last)
    {
        IsSelect = true;
        if (Last != null)
        {
            Last.IsSelect = false;
        }
        Last = last;
        Last.IsSelect = true;
    }

    public void Install()
    {
        Display = true;
        Load1();
    }

    public void Install1(FileDisplayObj data)
    {
        App.ShowAddGame();
        if (data.SourceType == SourceType.CurseForge)
        {
            App.AddGameWindow?.Install(
                (data.Data as CurseForgeObjList.Data.LatestFiles)!,
                (Last!.Data?.Data as CurseForgeObjList.Data)!);
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            App.AddGameWindow?.Install(
                (data.Data as ModrinthVersionObj)!,
                (Last!.Data?.Data as ModrinthSearchObj.Hit)!);
        }
        var window = Con.Window;
        window.Close();
    }

    private async void Load()
    {
        var window = Con.Window;
        if (Source == 2 && Categorie == 4
            && Text?.Length < 3)
        {
            window.OkInfo.Show(App.GetLanguage("AddModPackWindow.Error6"));
            Unlock();
            return;
        }

        window.ProgressInfo.Show(App.GetLanguage("AddModPackWindow.Info2"));
        var data = await WebBinding.GetPackList((SourceType)Source,
            GameVersion, Text, Page, Source == 2 ? Categorie : SortType,
            Source == 2 ? "" : Categorie < 0 ? "" : Categories[Categorie]);

        if (data == null)
        {
            window.OkInfo.Show(App.GetLanguage("AddModPackWindow.Error2"));
            window.ProgressInfo.Close();
            Unlock();
            return;
        }

        DisplayList.Clear();
        foreach (var item in data)
        {
            DisplayList.Add(new(item));
        }
        OnPropertyChanged(nameof(DisplayList));

        Last = null;

        EmptyDisplay = DisplayList.Count == 0;

        window.ProgressInfo.Close();
        Unlock();
    }

    private async void Load1()
    {
        var window = Con.Window;
        FileList.Clear();
        window.ProgressInfo.Show(App.GetLanguage("AddModPackWindow.Info3"));
        List<FileDisplayObj>? list = null;
        if (Source == 0)
        {
            PageEnable1 = true;
            list = await WebBinding.GetPackFile((SourceType)Source,
                (Last!.Data?.Data as CurseForgeObjList.Data)!.id.ToString(), Page1,
                GameVersion1, Loaders.Normal);
        }
        else if (Source == 1)
        {
            PageEnable1 = false;
            list = await WebBinding.GetPackFile((SourceType)Source,
                (Last!.Data?.Data as ModrinthSearchObj.Hit)!.project_id, Page1,
                GameVersion1, Loaders.Normal);
        }
        if (list == null)
        {
            window.OkInfo.Show(App.GetLanguage("AddModPackWindow.Error3"));
            window.ProgressInfo.Close();
            return;
        }
        FileList.AddRange(list);

        window.ProgressInfo.Close();
    }

    private void Lock()
    {
        Enable = false;

        TextEnable = false;
        PageEnable = false;

        IsSelect = false;
    }

    private void Unlock()
    {
        Enable = true;

        TextEnable = true;
        PageEnable = true;
    }
}
