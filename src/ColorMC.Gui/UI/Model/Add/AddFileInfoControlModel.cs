using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddBaseModel : IAddFileControl
{
    /// <summary>
    /// 文件列表
    /// </summary>
    public ObservableCollection<FileVersionItemModel> FileList { get; init; } = [];

    /// <summary>
    /// 选中的项目
    /// </summary>
    [ObservableProperty]
    public partial FileItemModel Last { get; set; }

    /// <summary>
    /// 文件列表当前页数
    /// </summary>
    [ObservableProperty]
    public partial int? PageVersion { get; set; } = 1;

    /// <summary>
    /// 文件列表最大页数
    /// </summary>
    [ObservableProperty]
    public partial int MaxPageVersion { get; set; }

    /// <summary>
    /// 文件列表游戏版本
    /// </summary>
    [ObservableProperty]
    public partial string? GameVersionDownload { get; set; }

    /// <summary>
    /// 是否没有版本
    /// </summary>
    [ObservableProperty]
    public partial bool EmptyVersionDisplay { get; set; }

    /// <summary>
    /// 是否显示项目详情
    /// </summary>
    [ObservableProperty]
    public partial bool DisplayItemInfo { get; set; }

    /// <summary>
    /// 选中的文件
    /// </summary>
    [ObservableProperty]
    public partial FileVersionItemModel Item { get; set; }

    /// <summary>
    /// 在那个状态栏中
    /// </summary>
    [ObservableProperty]
    public partial int SelectIndex { get; set; } = 1;

    /// <summary>
    /// 项目列表是否有下一页
    /// </summary>
    [ObservableProperty]
    public partial bool HaveNextVersionPage { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    [ObservableProperty]
    public partial bool HaveLastVersionPage { get; set; }
    protected abstract Loaders GameLoader { get; }

    partial void OnSelectIndexChanged(int value)
    {
        if (_load)
        {
            return;
        }

        if (value == 1)
        {
            LoadInfoVersion();
        }
    }

    /// <summary>
    /// 页数改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageVersionChanged(int? value)
    {
        if (_load)
        {
            return;
        }

        LoadInfoVersion();
    }

    /// <summary>
    /// 游戏版本改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionDownloadChanged(string? value)
    {
        if (_load)
        {
            return;
        }

        LoadInfoVersion();
    }

    /// <summary>
    /// 返回上一页
    /// </summary>
    [RelayCommand]
    public void LastVersionPage()
    {
        if (_load || PageVersion <= 1)
        {
            return;
        }

        PageVersion -= 1;
    }

    /// <summary>
    /// 下一页
    /// </summary>
    [RelayCommand]
    public void NextVersionPage()
    {
        if (_load)
        {
            return;
        }

        PageVersion += 1;
    }

    /// <summary>
    /// 搜索文件列表
    /// </summary>
    [RelayCommand]
    public void Search()
    {
        LoadInfoVersion();
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

        var res = await Window.ShowChoice(
            string.Format(LangUtils.Get("AddModPackWindow.Text17"), Item.Name));
        if (res)
        {
            Install(Item);
        }
    }

    public void CloseView()
    {
        DisplayItemInfo = false;
        FileList.Clear();
        SelectIndex = 1;
    }

    /// <summary>
    /// 加载项目文件版本列表
    /// </summary>
    public async void LoadVersion(SourceType type, string pid, FileType type1)
    {
        _load = true;
        FileList.Clear();
        var dialog = Window.ShowProgress(LangUtils.Get("AddModPackWindow.Text19"));
        var page = 1;
        PageVersion ??= 1;

        if (type == SourceType.CurseForge)
        {
            page = PageVersion ?? 1;
        }

        page--;
        var res = await WebBinding.GetFileListAsync(type, pid, page, GameVersionDownload, GameLoader, type1);
        MaxPageVersion = res.Count / 50;
        var list = res.List;

        //curseforge只有50个项目
        if (type == SourceType.CurseForge)
        {
            page = 0;
        }

        VersionPageLoad();

        if (list == null)
        {
            Window.Show(LangUtils.Get("AddModPackWindow.Text24"));
            Window.CloseDialog(dialog);
            _load = false;
            return;
        }

        //一页50
        int b = 0;
        for (int a = page * 50; a < list.Count; a++, b++)
        {
            if (b >= 50)
            {
                break;
            }
            var item = list[a];
            item.Add = this;
            item.AddFile = this;
            item.IsDownload = CheckVersionDownload(item);
            FileList.Add(item);
        }

        EmptyVersionDisplay = FileList.Count == 0;

        Window.CloseDialog(dialog);
        Window.Notify(LangUtils.Get("AddResourceWindow.Text24"));
        _load = false;
    }

    /// <summary>
    /// 选中版本
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileVersionItemModel item)
    {
        Item?.IsSelect = false;
        Item = item;
        item.IsSelect = true;
    }

    protected void VersionPageLoad()
    {
        HaveNextVersionPage = PageVersion < MaxPageVersion;
        HaveLastVersionPage = PageVersion > 1;
    }

    protected void LoadInfoVersion()
    {
        LoadVersion(Last.Obj.Source, Last.Obj.Pid, Last.Obj.Type);
    }

    protected abstract bool CheckVersionDownload(FileVersionItemModel model);
}
