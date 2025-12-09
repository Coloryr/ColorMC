using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏资源
/// </summary>
public partial class AddResourceControlModel : AddBaseModel, IAddControl
{
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

    public AddResourceControlModel(WindowModel model, GameSettingObj obj) : base(model)
    {
        _obj = obj;
        _useName = ToString() ?? "AddControlModel";
    }

    /// <summary>
    /// 下载类型选择
    /// </summary>
    /// <param name="value"></param>
    async partial void OnTypeChanged(int value)
    {
        if (!Display)
        {
            return;
        }

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

        Page = 1;

        FileList.Clear();
        SourceList.Clear();

        SourceTypeList.Clear();
        SourceTypeList.AddRange(WebBinding.GetSourceList(_now));
        SourceTypeList.ForEach(item => SourceList.Add(item.GetName()));

        _load = false;

        Source = 0;

        LoadSourceData();
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
        var type = SourceTypeList[Source];

        if (type is SourceType.CurseForge or SourceType.Modrinth)
        {
            SortTypeList.AddRange(type is SourceType.CurseForge
                ? LanguageUtils.GetCurseForgeSortTypes()
                : LanguageUtils.GetModrinthSortTypes());
            //获取支持的游戏版本和分类
            var dialog = Window.ShowProgress(LanguageUtils.Get("AddModPackWindow.Text20"));
            var list = type is SourceType.CurseForge
                ? await CurseForgeHelper.GetGameVersionsAsync()
                : await ModrinthHelper.GetGameVersionAsync();
            var list1 = type is SourceType.CurseForge
                ? await CurseForgeHelper.GetCategoriesAsync(_now)
                : await ModrinthHelper.GetCategoriesAsync(_now);
            Window.CloseDialog(dialog);
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

            LoadDisplayList();
        }
        //McMod搜索源
        else if (type == SourceType.McMod)
        {
            var dialog = Window.ShowProgress(LanguageUtils.Get("AddModPackWindow.Text20"));
            //获取分类
            var list = await ColorMCAPI.GetMcModGroupAsync();
            Window.CloseDialog(dialog);
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

            LoadDisplayList();
        }

        SourceLoad = true;
        _load = false;
    }

    /// <summary>
    /// 刷新项目列表
    /// </summary>
    public override async void LoadDisplayList()
    {
        var type = SourceTypeList[Source];

        var page = Page ?? 1;
        page--;

        var dialog = Window.ShowProgress(LanguageUtils.Get("AddResourceWindow.Text17"));
        if (type == SourceType.McMod)
        {
            //McMod搜索源
            var data = await WebBinding.SearchMcmodAsync(Text ?? "", page,
                _obj.Loader, GameVersion ?? "", _categories[Categorie], SortType);
            if (data == null)
            {
                Window.CloseDialog(dialog);
                Window.Show(LanguageUtils.Get("AddResourceWindow.Text26"));
                return;
            }

            ClearList();

            foreach (var item in data)
            {
                item.Add = this;
                item.Window = Window;
                item.NowDownload = GameManager.TestDowload(item.Obj);
                DisplayList.Add(item);
            }
        }
        else
        {
            //其他搜索源
            var res = await WebBinding.GetListAsync(_now, type, GameVersion, Text, page,
                SortType, Categorie < 0 ? "" : _categories[Categorie], _obj.Loader);

            MaxPage = res.TotalCount / 20;

            PageLoad();

            if (res.List == null)
            {
                Window.CloseDialog(dialog);
                Window.Show(LanguageUtils.Get("AddResourceWindow.Text26"));
                return;
            }

            ClearList();

            //模组分页需要特殊处理
            if (_now == FileType.Mod)
            {
                foreach (var item in res.List)
                {
                    item.Add = this;
                    item.Window = Window;
                    if (_obj.Mods.ContainsKey(item.Obj.Pid))
                    {
                        item.IsDownload = true;
                    }
                    item.NowDownload = GameManager.TestDowload(item.Obj);
                    DisplayList.Add(item);
                }
            }
            else
            {
                foreach (var item in res.List)
                {
                    item.Add = this;
                    item.Window = Window;
                    item.NowDownload = GameManager.TestDowload(item.Obj);
                    DisplayList.Add(item);
                }
            }
        }

        OnPropertyChanged(nameof(DisplayList));

        _lastSelect = null;

        EmptyDisplay = DisplayList.Count == 0;

        Window.CloseDialog(dialog);
        Window.Notify(LanguageUtils.Get("AddResourceWindow.Text24"));
    }

    /// <summary>
    /// 加载失败
    /// </summary>
    private async void LoadFail()
    {
        var res = await Window.ShowChoice(LanguageUtils.Get("AddModPackWindow.Text25"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < SourceTypeList.Count)
        {
            res = await Window.ShowChoice(LanguageUtils.Get("AddModPackWindow.Text21"));
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
        if (res == null)
        {
            Window.Show(LanguageUtils.Get("AddModPackWindow.Text23"));
            _load = false;
            return;
        }
        Last = res;
        DisplayItemInfo = true;

        _load = false;
    }

    public override void Close()
    {
        base.Close();
        if (OptifineDisplay)
        {
            Window.PopBack();
        }

        _close = true;
        _load = true;
        Window.RemoveChoiseData(_useName);
        _optifineList.Clear();
        DownloadOptifineList.Clear();
        FileList.Clear();
    }
}
