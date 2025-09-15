using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏资源
/// </summary>
public partial class AddControlModel : GameModel
{
    public const string NameScrollToHome = "Scroll:Home";

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
    /// 下载源列表
    /// </summary>
    private readonly List<string> _sourceTypeNameList =
    [
        SourceType.CurseForge.GetName(),
        SourceType.Modrinth.GetName(),
    ];

    /// <summary>
    /// 显示的下载类型列表
    /// </summary>
    public string[] TypeList { get; init; } = LanguageBinding.GetAddType();
    /// <summary>
    /// 显示的游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
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
    /// 显示的项目列表
    /// </summary>
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];

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
    /// <summary>
    /// 项目列表是否有下一页
    /// </summary>
    [ObservableProperty]
    private bool _nextPage;

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
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string? _name;

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

    /// <summary>
    /// 可以下载的文件类型
    /// </summary>
    private static readonly List<FileType> s_types =
    [
        FileType.Mod,
        FileType.World,
        FileType.Shaderpack,
        FileType.Resourcepack,
        FileType.DataPacks,
        FileType.Optifine
    ];

    private readonly string _useName;

    public AddControlModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {
        _useName = ToString() ?? "AddControlModel";
    }

    partial void OnIsUpgradeChanged(bool value)
    {
        ModDownloadText = App.Lang(value ? "AddWindow.Text15" : "AddWindow.Text7");
    }

    /// <summary>
    /// 下载类型选择
    /// </summary>
    /// <param name="value"></param>
    async partial void OnTypeChanged(int value)
    {
        if (!Display)
            return;

        if (s_types[Type] == FileType.Optifine)
        {
            await OptifineOpen();
            return;
        }

        _load = true;

        IsSelect = false;
        _now = s_types[Type];
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
            var res = await WebBinding.GetList(_now, type, GameVersion, Name, Page ?? 0,
                SortType, Categorie < 0 ? "" : _categories[Categorie], Obj.Loader);

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

            //模组分页需要特殊处理
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

        OnPropertyChanged(NameScrollToHome);

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
    /// 转到文件下载
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

        var res = await Model.ShowAsync(
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

    /// <summary>
    /// 加载下载源信息
    /// </summary>
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

        if (type is SourceType.CurseForge or SourceType.Modrinth)
        {
            SortTypeList.AddRange(type is SourceType.CurseForge
                ? LanguageBinding.GetCurseForgeSortTypes()
                : LanguageBinding.GetModrinthSortTypes());
            //获取支持的游戏版本和分类
            Model.Progress(App.Lang("AddModPackWindow.Info4"));
            var list = type is SourceType.CurseForge
                ? await GameBinding.GetCurseForgeGameVersions()
                : await GameBinding.GetModrinthGameVersions();
            var list1 = type is SourceType.CurseForge
                ? await GameBinding.GetCurseForgeCategories(_now)
                : await GameBinding.GetModrinthCategories(_now);
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

            //自动设置游戏版本
            if (GameVersionList.Contains(Obj.Version))
            {
                GameVersionOptifine = GameVersion = Obj.Version;
            }
            else
            {
                GameVersionOptifine = GameVersion = GameVersionList.FirstOrDefault();
            }

            SortType = type is SourceType.CurseForge ? 1 : 0;
            Categorie = 0;

            await GetList();
        }
        //McMod搜索源
        else if (type == SourceType.McMod)
        {
            Model.Progress(App.Lang("AddModPackWindow.Info4"));
            //获取分类
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
                GameVersionDownload = GameVersion = Obj.Version;
            }
            else
            {
                GameVersionDownload = GameVersion = GameVersionList.FirstOrDefault();
            }

            await GetList();
        }

        SourceLoad = true;
        _load = false;
    }

    /// <summary>
    /// 转到下载类型
    /// </summary>
    /// <param name="type">下载源</param>
    /// <param name="pid">项目ID</param>
    public async void GoFile(SourceType type, string pid)
    {
        //跳转到模组类型
        Type = 0;
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
    /// 刷新项目列表
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
        var res = await Model.ShowAsync(App.Lang("AddModPackWindow.Error4"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (DownloadSource < _sourceTypeList.Count)
        {
            res = await Model.ShowAsync(App.Lang("AddModPackWindow.Info5"));
            if (res)
            {
                DownloadSource++;
            }
        }
    }

    /// <summary>
    /// 转到文件类型
    /// </summary>
    /// <param name="file">文件类型</param>
    public async void GoTo(FileType file)
    {
        if (file == FileType.Optifine)
        {
            await OptifineOpen();
        }
        else
        {
            Type = s_types.IndexOf(file);
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
    public void GoSet()
    {
        Set = true;

        //跳转到模组
        Type = 0;
        DownloadSource = 0;
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
    /// 选择项目
    /// </summary>
    /// <param name="last"></param>
    public void SetSelect(FileItemModel last)
    {
        if (IsDownload)
        {
            return;
        }

        IsSelect = true;
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = last;
        _last.IsSelect = true;
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
