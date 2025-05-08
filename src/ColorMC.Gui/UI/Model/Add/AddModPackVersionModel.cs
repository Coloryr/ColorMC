using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddModPackControlModel
{
    /// <summary>
    /// 选中的文件
    /// </summary>
    [ObservableProperty]
    private FileVersionItemModel _item;

    /// <summary>
    /// 文件列表
    /// </summary>
    public ObservableCollection<FileVersionItemModel> FileList { get; init; } = [];

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

    ///// <summary>
    ///// 是否显示文件列表
    ///// </summary>
    //[ObservableProperty]
    //private bool _enableVersion = true;
    /// <summary>
    /// 是否显示文件列表
    /// </summary>
    [ObservableProperty]
    private bool _displayVersion = false;
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
    /// 游戏列表显示
    /// </summary>
    /// <param name="value"></param>
    partial void OnDisplayVersionChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                DisplayVersion = false;
            });
        }
        else
        {
            Model.PopBack();
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

        LoadVersion();
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

        LoadVersion();
    }

    /// <summary>
    /// 搜索文件列表
    /// </summary>
    [RelayCommand]
    public void Search()
    {
        LoadVersion();
    }

    /// <summary>
    /// 安装所选文件
    /// </summary>
    /// <param name="data"></param>
    public async void Install(FileVersionItemModel data)
    {
        if (data.IsDownload)
        {
            return;
        }

        var select = _last;
        string? group = WindowManager.AddGameWindow?.GetGroup();
        if (data.SourceType == SourceType.CurseForge)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Info8"));

            var res = await GameBinding.InstallCurseForge((data.Data as CurseForgeModObj.DataObj)!,
                (select!.Data as CurseForgeListObj.DataObj)!, group,
                ZipUpdate, GameRequest, GameOverwirte, UpdateProcess, PackState);
            Model.ProgressClose();

            if (!res.State)
            {
                Model.Show(App.Lang("AddGameWindow.Tab1.Error8"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Info8"));
            var res = await GameBinding.InstallModrinth((data.Data as ModrinthVersionObj)!,
                (select!.Data as ModrinthSearchObj.HitObj)!, group,
                ZipUpdate, GameRequest, GameOverwirte, UpdateProcess, PackState);
            Model.ProgressClose();

            if (!res.State)
            {
                Model.Show(App.Lang("AddGameWindow.Tab1.Error8"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
    }

    /// <summary>
    /// 加载项目文件版本列表
    /// </summary>
    private async void LoadVersion()
    {
        if (DisplayVersion == false)
        {
            return;
        }

        FileList.Clear();
        Model.Progress(App.Lang("AddModPackWindow.Info3"));
        List<FileVersionItemModel>? list = null;
        var page = 0;
        if (Source == 0)
        {
            var res = await WebBinding.GetFileList((SourceType)Source,
                (_last!.Data as CurseForgeListObj.DataObj)!.Id.ToString(), PageDownload ?? 0,
                GameVersionDownload, Loaders.Normal);
            list = res.List;
            MaxPageDownload = res.Count / 50;
        }
        else if (Source == 1)
        {
            var res = await WebBinding.GetFileList((SourceType)Source,
                (_last!.Data as ModrinthSearchObj.HitObj)!.ProjectId, PageDownload ?? 0,
                GameVersionDownload, Loaders.Normal);
            list = res.List;
            MaxPageDownload = res.Count / 50;
            page = PageDownload ?? 0;
        }

        EnableNextPageDownload = (MaxPageDownload - PageDownload) > 0;

        if (list == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error3"));
            Model.ProgressClose();
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
            var games = GameBinding.GetGames();
            if (games.Any(item1 => item1.ModPack && item1.ModPackType == (SourceType)Source
            && item1.PID == item.ID && item1.FID == item.ID1))
            {
                item.IsDownload = true;
            }
            FileList.Add(item);
        }

        EmptyVersionDisplay = FileList.Count == 0;

        Model.ProgressClose();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    /// <summary>
    /// 选中版本
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileVersionItemModel item)
    {
        if (Item != null)
        {
            Item.IsSelect = false;
        }
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
}
