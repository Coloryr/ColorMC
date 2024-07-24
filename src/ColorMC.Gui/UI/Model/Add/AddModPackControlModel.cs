using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddModPackControlModel : TopModel, IAddWindow
{
    public string[] SourceList { get; init; } = LanguageBinding.GetSourceList();
    public ObservableCollection<FileDisplayModel> FileList { get; init; } = [];
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    public ObservableCollection<string> CategorieList { get; init; } = [];
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];

    private readonly Dictionary<int, string> _categories = [];
    private FileItemModel? _last;
    private bool _load = false;

    [ObservableProperty]
    private FileDisplayModel _item;

    [ObservableProperty]
    private int _source = -1;
    [ObservableProperty]
    private int _categorie;
    [ObservableProperty]
    private int _sortType;
    [ObservableProperty]
    private int? _page = 0;
    [ObservableProperty]
    private int? _page1 = 0;
    [ObservableProperty]
    private string? _gameVersion;
    [ObservableProperty]
    private string? _gameVersion1;
    [ObservableProperty]
    private string? _text;
    [ObservableProperty]
    private bool _pageEnable1 = true;
    [ObservableProperty]
    private bool _enable1 = true;
    [ObservableProperty]
    private bool _isSelect = false;
    [ObservableProperty]
    private bool _display = false;
    [ObservableProperty]
    private bool _emptyDisplay = true;

    [ObservableProperty]
    private bool _sourceLoad;

    private double _count;

    private readonly string _useName;

    public AddModPackControlModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "AddModPackControlModel";
    }

    partial void OnDisplayChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                Display = false;
            });
            Model.HeadChoiseDisplay = false;
        }
        else
        {
            Model.HeadChoiseDisplay = true;
            Model.PopBack();
        }
    }

    partial void OnGameVersion1Changed(string? value)
    {
        Load1();
    }

    partial void OnPage1Changed(int? value)
    {
        Load1();
    }

    partial void OnCategorieChanged(int value)
    {
        if (_load)
            return;

        Load();
    }

    partial void OnSortTypeChanged(int value)
    {
        if (_load)
            return;

        Load();
    }

    partial void OnGameVersionChanged(string? value)
    {
        if (_load)
            return;


        GameVersion1 = value;

        Load();
    }

    partial void OnPageChanged(int? value)
    {
        if (_load)
            return;

        Load();
    }

    partial void OnSourceChanged(int value)
    {
        LoadSourceData();
    }

    [RelayCommand]
    public void Select()
    {
        if (_last == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error1"));
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
    public void Search()
    {
        Load1();
    }

    [RelayCommand]
    public async Task Download()
    {
        if (Item == null)
            return;

        var res = await Model.ShowWait(
            string.Format(App.Lang("AddModPackWindow.Info1"), Item.Name));
        if (res)
        {
            Install1(Item);
        }
    }

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
        _categories.Clear();

        ClearList();

        switch (Source)
        {
            case 0:
            case 1:
                SortTypeList.AddRange(Source == 0 ?
                    LanguageBinding.GetCurseForgeSortTypes() :
                    LanguageBinding.GetModrinthSortTypes());

                Model.Progress(App.Lang("AddModPackWindow.Info4"));
                var list = Source == 0 ?
                    await GameBinding.GetCurseForgeGameVersions() :
                    await GameBinding.GetModrinthGameVersions();
                var list1 = Source == 0 ?
                    await GameBinding.GetCurseForgeCategories() :
                    await GameBinding.GetModrinthCategories();
                Model.ProgressClose();
                if (list == null || list1 == null)
                {
                    _load = false;
                    LoadFail();
                    return;
                }
                GameVersionList.AddRange(list);

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

    public void SetSelect(FileItemModel last)
    {
        IsSelect = true;
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = last;
        _last.IsSelect = true;
    }

    public void Install()
    {
        Display = true;
        Load1();
    }

    public void Install1(FileDisplayModel data)
    {
        var select = _last;
        WindowClose();
        WindowManager.ShowAddGame(null);
        if (data.SourceType == SourceType.CurseForge)
        {
            WindowManager.AddGameWindow?.Install(
                (data.Data as CurseForgeModObj.Data)!,
                (select!.Data?.Data as CurseForgeObjList.Data)!);
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            WindowManager.AddGameWindow?.Install(
                (data.Data as ModrinthVersionObj)!,
                (select!.Data?.Data as ModrinthSearchObj.Hit)!);
        }
    }

    private async void LoadFail()
    {
        var res = await Model.ShowWait(App.Lang("AddModPackWindow.Error4"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < SourceList.Length)
        {
            res = await Model.ShowWait(App.Lang("AddModPackWindow.Info5"));
            if (res)
            {
                Source++;
            }
        }
    }

    private async void Load()
    {
        if (Source == 2 && Categorie == 4
            && Text?.Length < 3)
        {
            Model.Show(App.Lang("AddModPackWindow.Error6"));
            return;
        }

        Model.Progress(App.Lang("AddModPackWindow.Info2"));
        var data = await WebBinding.GetPackList((SourceType)Source,
            GameVersion, Text, Page ?? 0, Source == 2 ? Categorie : SortType,
            Source == 2 ? "" : Categorie < 0 ? "" : _categories[Categorie]);

        if (data == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error2"));
            Model.ProgressClose();
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

    private async void Load1()
    {
        if (Display == false)
            return;

        FileList.Clear();
        Model.Progress(App.Lang("AddModPackWindow.Info3"));
        List<FileDisplayModel>? list = null;
        if (Source == 0)
        {
            PageEnable1 = true;
            list = await WebBinding.GetPackFile((SourceType)Source,
                (_last!.Data?.Data as CurseForgeObjList.Data)!.id.ToString(), Page1 ?? 0,
                GameVersion1, Loaders.Normal);
        }
        else if (Source == 1)
        {
            PageEnable1 = false;
            list = await WebBinding.GetPackFile((SourceType)Source,
                (_last!.Data?.Data as ModrinthSearchObj.Hit)!.project_id, Page1 ?? 0,
                GameVersion1, Loaders.Normal);
        }
        if (list == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error3"));
            Model.ProgressClose();
            return;
        }
        FileList.AddRange(list);

        Model.ProgressClose();
    }

    public void Install(FileItemModel item)
    {
        SetSelect(item);
        Install();
    }

    private void ClearList()
    {
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
    }

    public void Back()
    {
        if (_load || Page <= 0)
        {
            return;
        }

        Page -= 1;
        _count = 0;
    }

    public void Next()
    {
        if (_load)
        {
            return;
        }

        Page += 1;
        _count = 0;
    }

    public void Reload1()
    {
        if (Display)
        {
            Load1();
        }
        else
        {
            Load();
        }
    }

    public void Wheel(double y)
    {
        _count += y;
        if (_count > 5)
        {
            Back();
        }
        else if (_count < -5)
        {
            Next();
        }
    }

    public override void Close()
    {
        if (Display)
        {
            Model.PopBack();
        }

        _load = true;
        Model.RemoveChoiseData(_useName);
        FileList.Clear();
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
        _last = null;
    }
}
