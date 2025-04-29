using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加整合包
/// </summary>
public partial class AddModPackControlModel : TopModel, IAddControl
{
    /// <summary>
    /// 下载源列表
    /// </summary>
    public string[] SourceList { get; init; } = LanguageBinding.GetSourceList();

    /// <summary>
    /// 游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    /// <summary>
    /// 分类列表
    /// </summary>
    public ObservableCollection<string> CategorieList { get; init; } = [];
    /// <summary>
    /// 排序列表
    /// </summary>
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    /// <summary>
    /// 项目列表
    /// </summary>
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];

    /// <summary>
    /// 分类
    /// </summary>
    private readonly Dictionary<int, string> _categories = [];
    /// <summary>
    /// 选中的项目
    /// </summary>
    private FileItemModel? _last;
    /// <summary>
    /// 是否在加载
    /// </summary>
    private bool _load = false;

    /// <summary>
    /// 下载源
    /// </summary>
    [ObservableProperty]
    private int _source = -1;
    /// <summary>
    /// 分类
    /// </summary>
    [ObservableProperty]
    private int _categorie;
    /// <summary>
    /// 排序
    /// </summary>
    [ObservableProperty]
    private int _sortType;
    /// <summary>
    /// 当前页数
    /// </summary>
    [ObservableProperty]
    private int? _page = 0;
    /// <summary>
    /// 最大页数
    /// </summary>
    [ObservableProperty]
    private int _maxPage;
    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersion;
    /// <summary>
    /// 搜索文本
    /// </summary>
    [ObservableProperty]
    private string? _text;
    /// <summary>
    /// 是否选中了项目
    /// </summary>
    [ObservableProperty]
    private bool _isSelect = false;
    /// <summary>
    /// 是否没有项目
    /// </summary>
    [ObservableProperty]
    private bool _emptyDisplay = true;
    /// <summary>
    /// 是否下载源加载数据
    /// </summary>
    [ObservableProperty]
    private bool _sourceLoad;
    /// <summary>
    /// 是否允许下一页
    /// </summary>
    [ObservableProperty]
    private bool _enableNextPage;
    /// <summary>
    /// 是否继续添加
    /// </summary>
    private bool _keep = false;

    private readonly string _useName;

    public AddModPackControlModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "AddModPackControlModel";
    }

    /// <summary>
    /// 分类改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnCategorieChanged(int value)
    {
        if (_load)
        {
            return;
        }

        Load();
    }

    /// <summary>
    /// 排序改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnSortTypeChanged(int value)
    {
        if (_load)
        {
            return;
        }

        Load();
    }

    /// <summary>
    /// 游戏版本改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionChanged(string? value)
    {
        if (_load)
        {
            return;
        }

        GameVersionDownload = value;

        Load();
    }

    /// <summary>
    /// 页数改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageChanged(int? value)
    {
        if (_load)
            return;

        Load();
    }

    /// <summary>
    /// 下载源改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnSourceChanged(int value)
    {
        LoadSourceData();
    }

    /// <summary>
    /// 选中项目
    /// </summary>
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

    /// <summary>
    /// 刷新项目列表
    /// </summary>
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

    /// <summary>
    /// 下载所选项目
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Download()
    {
        if (Item == null)
            return;

        var res = await Model.ShowAsync(
            string.Format(App.Lang("AddModPackWindow.Info1"), Item.Name));
        if (res)
        {
            Install(Item);
        }
    }

    /// <summary>
    /// 加载搜索源数据
    /// </summary>
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

    /// <summary>
    /// 选中项目
    /// </summary>
    /// <param name="last"></param>
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

    /// <summary>
    /// 开始安装项目
    /// </summary>
    public void Install()
    {
        DisplayVersion = true;
        LoadVersion();
    }

    /// <summary>
    /// 安装包解压
    /// </summary>
    /// <param name="text"></param>
    /// <param name="size"></param>
    /// <param name="all"></param>
    private void ZipUpdate(string text, int size, int all)
    {
        string temp = App.Lang("AddGameWindow.Tab1.Info21");
        Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {text} {size}/{all}"));
    }

    /// <summary>
    /// 请求显示内容，并确定返回值
    /// </summary>
    /// <param name="text">内容</param>
    /// <returns></returns>
    private async Task<bool> GameRequest(string text)
    {
        Model.ProgressClose();
        var test = await Model.ShowAsync(text);
        Model.Progress();
        return test;
    }

    /// <summary>
    /// 请求游戏实例覆盖
    /// </summary>
    /// <param name="obj">需要覆盖的游戏实例</param>
    /// <returns></returns>
    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        Model.ProgressClose();
        var test = await Model.ShowAsync(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
        Model.Progress();
        return test;
    }

    /// <summary>
    /// 添加进度
    /// </summary>
    /// <param name="state"></param>
    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            Model.ProgressUpdate(-1);
            if (!ConfigBinding.WindowMode())
            {
                Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info4"));
            }
            else
            {
                Model.ProgressClose();
            }
        }
        else if (state == CoreRunState.DownloadDone)
        {
            if (ConfigBinding.WindowMode())
            {
                Model.Progress(App.Lang("AddGameWindow.Tab2.Info4"));
            }
        }
    }

    /// <summary>
    /// 进度条
    /// </summary>
    /// <param name="size"></param>
    /// <param name="now"></param>
    private void UpdateProcess(int size, int now)
    {
        Model.ProgressUpdate((double)now / size);
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private async void Done(string? uuid)
    {
        Model.Notify(App.Lang("AddGameWindow.Tab1.Info7"));

        DisplayVersion = false;

        if (_keep)
        {
            return;
        }

        var model = WindowManager.MainWindow?.DataContext as MainModel;
        model?.Select(uuid);

        var res = await Model.ShowAsync(App.Lang("AddGameWindow.Tab1.Info25"));
        if (res != true)
        {
            Dispatcher.UIThread.Post(WindowClose);
        }
        else
        {
            _keep = true;
        }
    }

    /// <summary>
    /// 加载搜索源信息失败
    /// </summary>
    private async void LoadFail()
    {
        var res = await Model.ShowAsync(App.Lang("AddModPackWindow.Error4"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < SourceList.Length)
        {
            res = await Model.ShowAsync(App.Lang("AddModPackWindow.Info5"));
            if (res)
            {
                Source++;
            }
        }
    }

    /// <summary>
    /// 加载项目列表
    /// </summary>
    private async void Load()
    {
        //MO不允许少文字搜索
        if (Source == 1 && Categorie == 4 && Text?.Length < 3)
        {
            Model.Show(App.Lang("AddModPackWindow.Error6"));
            return;
        }

        Model.Progress(App.Lang("AddModPackWindow.Info2"));
        var res = await WebBinding.GetModPackList((SourceType)Source,
            GameVersion, Text, Page ?? 0, Source == 2 ? Categorie : SortType,
            Source == 2 ? "" : Categorie < 0 ? "" : _categories[Categorie]);

        //制作分页
        if (Source == 0)
        {
            MaxPage = res.Count / 20;
            EnableNextPage = (MaxPage - Page) > 0;
        }
        else
        {
            MaxPage = int.MaxValue;
            EnableNextPage = true;
        }

        var data = res.List;

        if (data == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error2"));
            Model.ProgressClose();
            return;
        }

        DisplayList.Clear();

        //一页20
        int b = 0;
        for (int a = 0; a < data.Count; a++, b++)
        {
            if (b >= 20)
            {
                break;
            }
            var item = data[a];
            item.Add = this;
            DisplayList.Add(item);
        }

        OnPropertyChanged(nameof(DisplayList));

        _last = null;

        EmptyDisplay = DisplayList.Count == 0;

        Model.ProgressClose();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    /// <summary>
    /// 安装所选项目
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileItemModel item)
    {
        SetSelect(item);
        Install();
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

    /// <summary>
    /// 上一页
    /// </summary>
    public void Back()
    {
        if (_load || Page <= 0)
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
        if (_load)
        {
            return;
        }

        Page += 1;
    }

    /// <summary>
    /// F5重载版本列表
    /// </summary>
    public void ReloadF5()
    {
        if (DisplayVersion)
        {
            LoadVersion();
        }
        else
        {
            Load();
        }
    }

    public override void Close()
    {
        if (DisplayVersion)
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
