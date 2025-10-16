using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ColorMC;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏资源
/// 文件列表
/// </summary>
public partial class AddControlModel
{
    /// <summary>
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<FileModVersionModel> DownloadModList { get; init; } = [];
    /// <summary>
    /// 显示的文件列表
    /// </summary>
    public ObservableCollection<FileVersionItemModel> FileList { get; init; } = [];

    /// <summary>
    /// 文件列表显示
    /// </summary>
    [ObservableProperty]
    private bool _versionDisplay;
    /// <summary>
    /// 是否没有项目
    /// </summary>
    [ObservableProperty]
    private bool _emptyVersionDisplay = true;
    /// <summary>
    /// 模组列表显示
    /// </summary>
    [ObservableProperty]
    private bool _modDownloadDisplay;
    /// <summary>
    /// 展示所有附属模组
    /// </summary>
    [ObservableProperty]
    private bool _loadMoreMod;
    /// <summary>
    /// 文件列表是否显示下一页
    /// </summary>
    [ObservableProperty]
    private bool _nextPageDisplay;

    /// <summary>
    /// 文件列表当前页数
    /// </summary>
    [ObservableProperty]
    private int? _pageDownload;
    /// <summary>
    /// 文件最大页数
    /// </summary>
    [ObservableProperty]
    private int _maxPageDownload;

    /// <summary>
    /// 文件列表游戏版本
    /// </summary>
    [ObservableProperty]
    private string? _gameVersionDownload;
    /// <summary>
    /// 模组扩展选项文字
    /// </summary>
    [ObservableProperty]
    private string? _modDownloadText = App.Lang("AddWindow.Text7");

    /// <summary>
    /// 项目
    /// </summary>
    [ObservableProperty]
    private FileVersionItemModel? _file;
    /// <summary>
    /// 下载的模组
    /// </summary>
    [ObservableProperty]
    private FileModVersionModel? _mod;

    /// <summary>
    /// 加载更多模组
    /// </summary>
    /// <param name="value"></param>
    partial void OnLoadMoreModChanged(bool value)
    {
        ModsLoad(true);
    }

    /// <summary>
    /// 文件列表显示
    /// </summary>
    /// <param name="value"></param>
    partial void OnVersionDisplayChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                _lastId = null;
                VersionDisplay = false;
            });
        }
        else
        {
            Model.PopBack();
        }
    }

    /// <summary>
    /// 文件列表页数修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageDownloadChanged(int? value)
    {
        if (!Display || _load)
        {
            return;
        }

        if (_lastSelect != null)
        {
            LoadFile(_lastSelect);
        }
    }

    /// <summary>
    /// 文件列表游戏版本修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionDownloadChanged(string? value)
    {
        if (!Display || _load)
        {
            return;
        }

        if (_lastSelect != null)
        {
            LoadFile(_lastSelect);
        }
    }

    /// <summary>
    /// 打开文件列表
    /// </summary>
    private void InstallItem(FileItemModel item)
    {
        _lastType = SourceType.McMod;
        _lastId = null;

        _load = true;
        PageDownload = 0;
        LoadFile(item);
        _load = false;
    }

    /// <summary>
    /// 加载文件列表
    /// </summary>
    private async void LoadFile(FileItemModel item)
    {
        FileList.Clear();

        var type = _sourceTypeList[DownloadSource];
        if (type == SourceType.McMod)
        {
            if (_lastType == SourceType.McMod)
            {
                var obj1 = (item.Data as McModSearchItemObj)!;
                if (obj1.CurseforgeId != null && obj1.ModrinthId != null)
                {
                    var res = await Model.ShowCombo(App.Lang("AddWindow.Info14"), _sourceTypeNameList);
                    if (res.Cancel)
                    {
                        return;
                    }
                    _lastType = type = res.Index == 0 ? SourceType.CurseForge : SourceType.Modrinth;
                    _lastId = type == SourceType.CurseForge ? obj1.CurseforgeId : obj1.ModrinthId;
                }
                else if (obj1.CurseforgeId != null)
                {
                    _lastId = obj1.CurseforgeId;
                    _lastType = type = SourceType.CurseForge;
                }
                else if (obj1.ModrinthId != null)
                {
                    _lastId = obj1.ModrinthId;
                    _lastType = type = SourceType.Modrinth;
                }
            }
            else
            {
                type = _lastType;
            }
        }

        if (type == SourceType.McMod)
        {
            Model.Show(App.Lang("AddWindow.Error11"));
            return;
        }

        VersionDisplay = true;
        var page = 0;
        List<FileVersionItemModel>? list = null;
        Model.Progress(App.Lang("AddWindow.Info3"));
        if (type == SourceType.CurseForge)
        {
            var res = await WebBinding.GetFileListAsync(type, _lastId ??
                (item.Data as CurseForgeListObj.CurseForgeListDataObj)!.Id.ToString(), PageDownload ?? 0,
                GameVersionDownload, Obj.Loader, _now);
            list = res.List;
            MaxPageDownload = res.Count / 50;
        }
        else if (type == SourceType.Modrinth)
        {
            var res = await WebBinding.GetFileListAsync(type, _lastId ??
                (item.Data as ModrinthSearchObj.HitObj)!.ProjectId, 0,
                GameVersionDownload, _now == FileType.Mod ? Obj.Loader : Loaders.Normal, _now);
            list = res.List;
            MaxPageDownload = res.Count / 50;
            page = PageDownload ?? 0;
        }

        NextPageDisplay = (MaxPageDownload - PageDownload) > 0;

        if (list == null)
        {
            Model.Show(App.Lang("AddWindow.Error3"));
            Model.ProgressClose();
            return;
        }

        if (_now == FileType.Mod)
        {
            int b = 0;
            for (int a = page * 50; a < list.Count; a++, b++)
            {
                if (b >= 50)
                {
                    break;
                }
                var item1 = list[a];
                item1.Add = this;
                if (Obj.Mods.TryGetValue(item1.ID, out var value)
                    && value.FileId == item1.ID1)
                {
                    item1.IsDownload = true;
                }
                FileList.Add(item1);
            }
        }
        else
        {
            int b = 0;
            for (int a = page * 50; a < list.Count; a++, b++)
            {
                if (b >= 50)
                {
                    break;
                }
                var item1 = list[a];
                item1.Add = this;
                FileList.Add(item1);
            }
        }

        EmptyVersionDisplay = FileList.Count == 0;

        Model.ProgressClose();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    /// <summary>
    /// 开始下载文件
    /// </summary>
    /// <param name="data"></param>
    public async void Install(FileVersionItemModel data)
    {
        if (data.IsDownload)
        {
            return;
        }

        var type = _sourceTypeList[DownloadSource];
        if (Set)
        {
            if (type == SourceType.CurseForge)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as CurseForgeModObj.CurseForgeDataObj);
            }
            else if (type == SourceType.Modrinth)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as ModrinthVersionObj);
            }
            return;
        }

        ModInfoObj? mod = null;
        if (_now == FileType.Mod && Obj.Mods.TryGetValue(data.ID, out mod))
        {
            var res1 = await Model.ShowAsync(App.Lang("AddWindow.Info15"));
            if (!res1)
            {
                return;
            }
        }

        VersionDisplay = false;
        bool res = false;

        GameManager.StartAdd(Obj.UUID);
        try
        {
            //数据包
            if (_now == FileType.DataPacks)
            {
                //选择存档
                var list = await GameBinding.GetWorldsAsync(Obj);
                if (list.Count == 0)
                {
                    Model.Show(App.Lang("AddWindow.Error6"));
                    return;
                }

                var world = new List<string>();
                list.ForEach(item => world.Add(item.LevelName));
                var res1 = await Model.ShowCombo(App.Lang("AddWindow.Info7"), world);
                if (res1.Cancel)
                {
                    return;
                }
                var item = list[res1.Index];

                try
                {
                    DownloadItemInfo? info = null;
                    if (type == SourceType.CurseForge
                        && data.Data is CurseForgeModObj.CurseForgeDataObj data1)
                    {
                        info = new DownloadItemInfo
                        {
                            Type = FileType.DataPacks,
                            Source = type,
                            PID = data1.ModId.ToString()
                        };
                        StartDownload(info);

                        res = await WebBinding.DownloadAsync(item, data1);
                    }
                    else if (type == SourceType.Modrinth
                        && data.Data is ModrinthVersionObj data2)
                    {
                        info = new DownloadItemInfo
                        {
                            Type = FileType.DataPacks,
                            Source = type,
                            PID = data2.ProjectId
                        };
                        StartDownload(info);

                        res = await WebBinding.DownloadAsync(item, data2);
                    }
                    if (info != null)
                    {
                        StopDownload(info, res);
                    }
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("AddWindow.Error7"), e);
                    res = false;
                }
            }
            //模组
            else if (_now == FileType.Mod)
            {
                try
                {
                    var list = (type == SourceType.McMod ? _lastType : type) switch
                    {
                        SourceType.CurseForge => await WebBinding.GetDownloadModListAsync(Obj,
                        data.Data as CurseForgeModObj.CurseForgeDataObj),
                        SourceType.Modrinth => await WebBinding.GetDownloadModListAsync(Obj,
                        data.Data as ModrinthVersionObj),
                        _ => null
                    };
                    if (list == null)
                    {
                        Model.Show(App.Lang("AddWindow.Error9"));
                        return;
                    }

                    if (list.List!.Count == 0)
                    {
                        var info = new DownloadItemInfo
                        {
                            Type = FileType.Mod,
                            Source = type,
                            PID = list.Info.ModId
                        };
                        StartDownload(info);

                        res = await WebBinding.DownloadModAsync(Obj,
                        [
                            new DownloadModArg()
                            {
                                Item = list.Item!,
                                Info = list.Info!,
                                Old = await Obj.ReadModAsync(mod)
                            }
                        ]);

                        StopDownload(info, res);
                    }
                    else
                    {
                        res = await StartListTask(list, mod, type);
                    }
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("AddWindow.Error8"), e);
                    res = false;
                }
            }
            else
            {
                try
                {
                    res = type switch
                    {
                        SourceType.CurseForge => await WebBinding.DownloadAsync(_now, Obj,
                            data.Data as CurseForgeModObj.CurseForgeDataObj),
                        SourceType.Modrinth => await WebBinding.DownloadAsync(_now, Obj,
                            data.Data as ModrinthVersionObj),
                        _ => false
                    };
                }
                catch (Exception e)
                {
                    Logs.Error(App.Lang("AddWindow.Error8"), e);
                    res = false;
                }
            }

            //下载结束
            if (res)
            {
                Model.Notify(App.Lang("Text.Downloaded"));
            }
            else
            {
                Model.Show(App.Lang("AddWindow.Error5"));
            }
        }
        finally
        {
            GameManager.StopAdd(Obj.UUID);
        }
    }
}
