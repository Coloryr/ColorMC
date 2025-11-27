using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddFileInfoControlModel(WindowModel model, IAddControl add) : ObservableObject, IAddFileControl
{
    /// <summary>
    /// 游戏版本列表
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];

    /// <summary>
    /// 文件列表
    /// </summary>
    public ObservableCollection<FileVersionItemModel> FileList { get; init; } = [];

    /// <summary>
    /// 选中的项目
    /// </summary>
    [ObservableProperty]
    private FileItemModel? _last;

    /// <summary>
    /// 文件列表当前页数
    /// </summary>
    [ObservableProperty]
    private int? _pageDownload = 0;
    /// <summary>
    /// 文件列表最大页数
    /// </summary>
    [ObservableProperty]
    private int _maxPageDownload;
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
    /// 是否允许文件列表下一页
    /// </summary>
    [ObservableProperty]
    private bool _enableNextPageDownload;

    /// <summary>
    /// 是否为小界面模式
    /// </summary>
    [ObservableProperty]
    private bool _minMode;

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

    private bool _load;

    private SourceType _type;
    private string _pid;

    partial void OnSelectIndexChanged(int value)
    {
        if (_load)
        {
            return;
        }

        if (value == 1)
        {
            LoadVersion(_type, _pid);
        }
    }

    /// <summary>
    /// 页数改变
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageDownloadChanged(int? value)
    {
        if (_load)
        {
            return;
        }

        LoadVersion(_type, _pid);
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

        LoadVersion(_type, _pid);
    }

    /// <summary>
    /// 搜索文件列表
    /// </summary>
    [RelayCommand]
    public void Search()
    {
        LoadVersion(_type, _pid);
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

        var res = await model.ShowChoice(
            string.Format(LanguageUtils.Get("AddModPackWindow.Text17"), Item.Name));
        if (res)
        {
            add.Install(Item);
        }
    }

    /// <summary>
    /// 加载项目文件版本列表
    /// </summary>
    public async void LoadVersion(SourceType type, string pid)
    {
        _load = true;
        FileList.Clear();
        var dialog = model.ShowProgress(LanguageUtils.Get("AddModPackWindow.Text19"));
        List<FileVersionItemModel>? list = null;
        var page = 0;
        PageDownload ??= 0;

        if (type == SourceType.CurseForge)
        {
            page = PageDownload ?? 0;
        }

        var res = await WebBinding.GetFileListAsync(type,
               pid, page,
                GameVersionDownload, Loaders.Normal);
        MaxPageDownload = res.Count / 50;
        list = res.List;
        var title = res.Name;

        //curseforge只有50个项目
        if (type == SourceType.CurseForge)
        {
            page = 0;
        }

        EnableNextPageDownload = (MaxPageDownload - PageDownload) > 0;

        if (list == null)
        {
            model.Show(LanguageUtils.Get("AddModPackWindow.Text24"));
            model.CloseDialog(dialog);
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
            item.Add = add;
            item.AddFile = this;
            var games = InstancesPath.Games;
            if (games.Any(item1 => item1.ModPack && item1.ModPackType == type
            && item1.PID == item.ID && item1.FID == item.ID1))
            {
                item.IsDownload = true;
            }
            FileList.Add(item);
        }

        EmptyVersionDisplay = FileList.Count == 0;

        model.CloseDialog(dialog);
        model.Notify(LanguageUtils.Get("AddResourceWindow.Text24"));
        model.Title = LanguageUtils.Get("AddModPackWindow.Title") + ": " + title;
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

    /// <summary>
    /// 返回上一页
    /// </summary>
    public void BackVersion()
    {
        if (_load || PageDownload <= 0)
        {
            return;
        }

        PageDownload -= 1;
    }

    /// <summary>
    /// 下一页
    /// </summary>
    public void NextVersion()
    {
        if (_load)
        {
            return;
        }

        PageDownload += 1;
    }

    public void Load(FileItemModel? model)
    {
        SelectIndex = 0;
        Last = model;
        if (model != null)
        {
            _type = model.SourceType;
            _pid = model.ID;
        }
    }

    public void Close()
    {
        Last = null;
    }
}
