using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏资源
/// </summary>
public partial class AddResourceControlModel : AddBaseModel, IAddControl
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
    public string[] TypeList { get; init; } = LanguageUtils.GetAddType();
    
    /// <summary>
    /// 是否为模组升级模式
    /// </summary>
    [ObservableProperty]
    private bool _isUpgrade;

    /// <summary>
    /// 下载类型
    /// </summary>
    [ObservableProperty]
    private int _type = -1;

    /// <summary>
    /// 当前文件类型
    /// </summary>
    private FileType _now;

    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj _obj;

    /// <summary>
    /// 可以下载的文件类型
    /// </summary>
    private static readonly List<FileType> s_types =
    [
        FileType.Mod,
        FileType.Save,
        FileType.Shaderpack,
        FileType.Resourcepack,
        FileType.DataPacks,
        FileType.Optifine
    ];

    private readonly string _useName;

    public AddResourceControlModel(BaseModel model, GameSettingObj obj) : base(model)
    {
        _obj = obj;
        _useName = ToString() ?? "AddControlModel";
    }

    partial void OnIsUpgradeChanged(bool value)
    {
        ModDownloadText = LanguageUtils.Get(value ? "AddResourceWindow.Text15" : "AddResourceWindow.Text7");
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
        SourceList.Clear();

        _sourceTypeList.Clear();
        _sourceTypeList.AddRange(WebBinding.GetSourceList(_now));
        _sourceTypeList.ForEach(item => SourceList.Add(item.GetName()));

        _load = false;

        Source = 0;
    }

    /// <summary>
    /// 获取项目列表
    /// </summary>
    [RelayCommand]
    public async Task GetList()
    {
        var type = _sourceTypeList[Source];

        Model.Progress(LanguageUtils.Get("AddResourceWindow.Text17"));
        if (type == SourceType.McMod)
        {
            //McMod搜索源
            var data = await WebBinding.SearchMcmodAsync(Text ?? "", Page ?? 0, _obj.Loader, GameVersion ?? "", _categories[Categorie], SortType);
            if (data == null)
            {
                Model.ProgressClose();
                Model.Show(LanguageUtils.Get("AddResourceWindow.Text26"));
                return;
            }

            ClearList();

            foreach (var item in data)
            {
                item.Add = this;
                TestFileItem(item);
                DisplayList.Add(item);
            }

            _lastSelect = null;

            EmptyDisplay = DisplayList.Count == 0;

            Model.ProgressClose();
        }
        else
        {
            //其他搜索源
            var res = await WebBinding.GetListAsync(_now, type, GameVersion, Text, Page ?? 0,
                SortType, Categorie < 0 ? "" : _categories[Categorie], _obj.Loader);

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
                Model.Show(LanguageUtils.Get("AddResourceWindow.Text26"));
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
                    if (_obj.Mods.ContainsKey(item.ID))
                    {
                        item.IsDownload = true;
                    }
                    TestFileItem(item);
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
                    TestFileItem(item);
                    DisplayList.Add(item);
                }
            }

            _lastSelect = null;

            EmptyDisplay = DisplayList.Count == 0;

            Model.ProgressClose();
        }

        OnPropertyChanged(NameScrollToHome);

        Model.Notify(LanguageUtils.Get("AddResourceWindow.Text24"));
    }

    /// <summary>
    /// 根据名字刷新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task GetNameList()
    {
        if (!string.IsNullOrWhiteSpace(Text) && Page != 0)
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

        var res = await Model.ShowAsync(string.Format(LanguageUtils.Get("AddResourceWindow.Text16"), item.Name));
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
        if (_lastSelect != null)
        {
            LoadVersions(_lastSelect);
        }
    }

    /// <summary>
    /// 转到文件列表
    /// </summary>
    [RelayCommand]
    public void GoInstall()
    {
        if (_lastSelect == null)
        {
            Model.Show(LanguageUtils.Get("AddResourceWindow.Text25"));
            return;
        }

        InstallItem(_lastSelect);
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
    public override async void LoadSourceData()
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
        var type = _sourceTypeList[Source];

        if (type is SourceType.CurseForge or SourceType.Modrinth)
        {
            SortTypeList.AddRange(type is SourceType.CurseForge
                ? LanguageUtils.GetCurseForgeSortTypes()
                : LanguageUtils.GetModrinthSortTypes());
            //获取支持的游戏版本和分类
            Model.Progress(LanguageUtils.Get("AddModPackWindow.Text20"));
            var list = type is SourceType.CurseForge
                ? await CurseForgeHelper.GetGameVersionsAsync()
                : await ModrinthHelper.GetGameVersionAsync();
            var list1 = type is SourceType.CurseForge
                ? await CurseForgeHelper.GetCategoriesAsync(_now)
                : await ModrinthHelper.GetCategoriesAsync(_now);
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
            if (GameVersionList.Contains(_obj.Version))
            {
                GameVersionOptifine = GameVersion = GameVersionDownload = _obj.Version;
            }
            else
            {
                GameVersionOptifine = GameVersion = GameVersionDownload = GameVersionList.FirstOrDefault();
            }

            SortType = type is SourceType.CurseForge ? 1 : 0;
            Categorie = 0;

            await GetList();
        }
        //McMod搜索源
        else if (type == SourceType.McMod)
        {
            Model.Progress(LanguageUtils.Get("AddModPackWindow.Text20"));
            //获取分类
            var list = await ColorMCAPI.GetMcModGroupAsync();
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

            //自动设置游戏版本
            if (GameVersionList.Contains(_obj.Version))
            {
                GameVersionOptifine = GameVersion = GameVersionDownload = _obj.Version;
            }
            else
            {
                GameVersionOptifine = GameVersion = GameVersionDownload = GameVersionList.FirstOrDefault();
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
        Source = (int)type;
        await Task.Run(() =>
        {
            while ((!Display || _load) && !_close)
            {
                Thread.Sleep(100);
            }
        });

        _load = true;

        var res = await WebBinding.GetFileItemAsync(type, pid, FileType.Mod);
        if (res != null)
        {
            _lastSelect = res;
            LoadVersions(_lastSelect);
        }

        _load = false;
    }

    /// <summary>
    /// 刷新项目列表
    /// </summary>
    public override async void LoadDisplayList()
    {
        await GetList();
    }

    /// <summary>
    /// 加载失败
    /// </summary>
    private async void LoadFail()
    {
        var res = await Model.ShowAsync(LanguageUtils.Get("AddModPackWindow.Text25"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < _sourceTypeList.Count)
        {
            res = await Model.ShowAsync(LanguageUtils.Get("AddModPackWindow.Text21"));
            if (res)
            {
                Source++;
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
            Source = 0;
        }
    }

    /// <summary>
    /// 重载
    /// </summary>
    public void Reload()
    {
        if (DisplayVersion)
        {
            Refresh1();
        }
        else
        {
            LoadDisplayList();
        }
    }

    /// <summary>
    /// 打开文件列表
    /// </summary>
    /// <param name="item">项目</param>
    public override void Install(FileItemModel item)
    {
        SetSelect(item);
        InstallItem(item);
    }

    public override void Close()
    {
        base.Close();
        if (DisplayVersion || OptifineDisplay || ModDownloadDisplay)
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
    }
}
