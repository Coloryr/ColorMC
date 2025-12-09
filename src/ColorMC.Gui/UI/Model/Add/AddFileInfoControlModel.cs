using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
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
    private FileItemModel _last;

    /// <summary>
    /// 文件列表当前页数
    /// </summary>
    [ObservableProperty]
    private int? _pageVersion = 1;
    /// <summary>
    /// 文件列表最大页数
    /// </summary>
    [ObservableProperty]
    private int _maxPageVersion;
    /// <summary>
    /// 文件列表游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersionDownload;

    /// <summary>
    /// 是否没有版本
    /// </summary>
    [ObservableProperty]
    private bool _emptyVersionDisplay;
    /// <summary>
    /// 是否显示项目详情
    /// </summary>
    [ObservableProperty]
    private bool _displayItemInfo;

    /// <summary>
    /// 选中的文件
    /// </summary>
    [ObservableProperty]
    private FileVersionItemModel _item;
    /// <summary>
    /// 在那个状态栏中
    /// </summary>
    [ObservableProperty]
    private int _selectIndex = 0;

    /// <summary>
    /// 项目列表是否有下一页
    /// </summary>
    [ObservableProperty]
    private bool _haveNextVersionPage;
    /// <summary>
    /// 是否有上一页
    /// </summary>
    [ObservableProperty]
    private bool _haveLastVersionPage;

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

    [RelayCommand]
    public void CloseView()
    {
        DisplayItemInfo = false;
        FileList.Clear();
        SelectIndex = 0;
    }

    /// <summary>
    /// 加载项目文件版本列表
    /// </summary>
    public async void LoadVersion(SourceType type, string pid)
    {
        _load = true;
        FileList.Clear();
        var dialog = Window.ShowProgress(LangUtils.Get("AddModPackWindow.Text19"));
        List<FileVersionItemModel>? list = null;
        var page = 1;
        PageVersion ??= 1;

        if (type == SourceType.CurseForge)
        {
            page = PageVersion ?? 1;
        }

        page--;
        var res = await WebBinding.GetFileListAsync(type,
               pid, page,
                GameVersionDownload, Loaders.Normal);
        MaxPageVersion = res.Count / 50;
        list = res.List;
        var title = res.Name;

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
            var games = InstancesPath.Games;
            if (games.Any(item1 => item1.Modpack && item1.ModPackType == type
                && item1.PID == item.Obj.Pid && item1.FID == item.Obj.Fid))
            {
                item.IsDownload = true;
            }
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
        LoadVersion(Last.Obj.Source, Last.Obj.Pid);
    }
}
