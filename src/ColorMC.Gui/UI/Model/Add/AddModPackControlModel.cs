using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddModPackControlModel : TopModel, IAddWindow
{
    public string[] SourceList { get; init; } = LanguageBinding.GetSourceList();
    public ObservableCollection<FileDisplayObj> FileList { get; init; } = [];
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    public ObservableCollection<string> CategorieList { get; init; } = [];
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];

    private readonly Dictionary<int, string> _categories = [];
    private FileItemModel? _last;
    private bool _load = false;

    [ObservableProperty]
    private FileDisplayObj _item;

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
    private bool _textEnable = true;
    [ObservableProperty]
    private bool _pageEnable = true;
    [ObservableProperty]
    private bool _pageEnable1 = true;
    [ObservableProperty]
    private bool _enable = true;
    [ObservableProperty]
    private bool _enable1 = true;
    [ObservableProperty]
    private bool _isSelect = false;
    [ObservableProperty]
    private bool _display = false;
    [ObservableProperty]
    private bool _emptyDisplay = true;
    [ObservableProperty]
    private bool _displayFilter = true;

    private double _count;

    private readonly string _useName;

    public AddModPackControlModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "AddModPackControlModel";
        Model.SetChoiseContent(_useName, App.Lang("Button.Filter"));
        Model.SetChoiseCall(_useName, choise: () =>
        {
            DisplayFilter = !DisplayFilter;
        });
    }

    partial void OnDisplayChanged(bool value)
    {
        if (value)
        {
            Model.AddBackCall(back: () =>
            {
                Display = false;
            });
            Model.HeadChoiseDisplay = false;
        }
        else
        {
            Model.HeadChoiseDisplay = true;
            Model.RemoveBack();
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

    async partial void OnSourceChanged(int value)
    {
        _load = true;

        Lock();

        CategorieList.Clear();
        SortTypeList.Clear();

        GameVersionList.Clear();
        _categories.Clear();

        DisplayList.Clear();
        OnPropertyChanged(nameof(DisplayList));

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
                if (list == null)
                {
                    Model.ShowOk(App.Lang("AddModPackWindow.Error4"), LoadFail);
                    return;
                }
                var list1 = Source == 0 ?
                    await GameBinding.GetCurseForgeCategories() :
                    await GameBinding.GetModrinthCategories();
                Model.ProgressClose();
                if (list1 == null)
                {
                    Model.ShowOk(App.Lang("AddModPackWindow.Error4"), LoadFail);
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

        _load = false;
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

    public void Install1(FileDisplayObj data)
    {
        var select = _last;
        WindowClose();
        App.ShowAddGame(null);
        if (data.SourceType == SourceType.CurseForge)
        {
            App.AddGameWindow?.Install(
                (data.Data as CurseForgeModObj.Data)!,
                (select!.Data?.Data as CurseForgeObjList.Data)!);
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            App.AddGameWindow?.Install(
                (data.Data as ModrinthVersionObj)!,
                (select!.Data?.Data as ModrinthSearchObj.Hit)!);
        }
    }

    private void LoadFail()
    {
        if (Source == 1)
        {
            WindowClose();
        }
        else
        {
            Source++;
        }
    }

    private async void Load()
    {
        if (Source == 2 && Categorie == 4
            && Text?.Length < 3)
        {
            Model.Show(App.Lang("AddModPackWindow.Error6"));
            Unlock();
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
            Unlock();
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
        Unlock();
    }

    private async void Load1()
    {
        if (Display == false)
            return;

        FileList.Clear();
        Model.Progress(App.Lang("AddModPackWindow.Info3"));
        List<FileDisplayObj>? list = null;
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

    public void Install(FileItemModel item)
    {
        SetSelect(item);
        Install();
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

    protected override void Close()
    {
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
