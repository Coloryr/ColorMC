using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ColorMC;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

        LoadFile();
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

        LoadFile();
    }

    /// <summary>
    /// 下载模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadMod()
    {
        Model.Progress(App.Lang("AddWindow.Info5"));
        bool res;
        if (DownloadModList.Any(item => item is ModUpgradeModel))
        {
            var list = DownloadModList.Where(item => item is ModUpgradeModel && item.Download)
                .Select(item => new DownloadModArg()
                {
                    Info = item.Items[item.SelectVersion].Info,
                    Item = item.Items[item.SelectVersion].Item,
                    Old = (item as ModUpgradeModel)!.Obj
                }).ToList();

            res = await WebBinding.DownloadMod(Obj, list);
        }
        else
        {
            var list = DownloadModList.Where(item => item.Download)
                                .Select(item => item.Items[item.SelectVersion]).ToList();
            if (_modsave != null)
            {
                list.Add(_modsave);
            }

            res = await WebBinding.DownloadMod(Obj, list);
        }
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("AddWindow.Error5"));
            if (_last != null)
            {
                _last.IsDownload = false;
                _last.NowDownload = false;
            }
        }
        else
        {
            if (_last != null)
            {
                _last.NowDownload = false;
                _last.IsDownload = true;
            }
        }
        CloseModDownloadDisplay();
    }

    /// <summary>
    /// 选择下载所有模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadAllMod()
    {
        foreach (var item in DownloadModList)
        {
            item.Download = true;
        }
        await DownloadMod();
    }

    /// <summary>
    /// 打开文件列表
    /// </summary>
    public void Install()
    {
        if (IsDownload)
        {
            Model.Show(App.Lang("AddWindow.Info9"));
            return;
        }

        _lastType = SourceType.McMod;
        _lastId = null;

        _load = true;
        PageDownload = 0;
        LoadFile();
        _load = false;
    }

    /// <summary>
    /// 加载文件列表
    /// </summary>
    private async void LoadFile()
    {
        FileList.Clear();

        var type = _sourceTypeList[DownloadSource];
        if (type == SourceType.McMod)
        {
            if (_lastType == SourceType.McMod)
            {
                var obj1 = (_last!.Data as McModSearchItemObj)!;
                if (obj1.CurseforgeId != null && obj1.ModrinthId != null)
                {
                    var res = await Model.Combo(App.Lang("AddWindow.Info14"), _sourceTypeNameList);
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
            var res = await WebBinding.GetFileList(type, _lastId ??
                (_last!.Data as CurseForgeListObj.DataObj)!.Id.ToString(), PageDownload ?? 0,
                GameVersionDownload, Obj.Loader, _now);
            list = res.List;
            MaxPageDownload = res.Count / 50;
        }
        else if (type == SourceType.Modrinth)
        {
            var res = await WebBinding.GetFileList(type, _lastId ??
                (_last!.Data as ModrinthSearchObj.HitObj)!.ProjectId, 0,
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
                var item = list[a];
                item.Add = this;
                if (Obj.Mods.TryGetValue(item.ID, out var value)
                    && value.FileId == item.ID1)
                {
                    item.IsDownload = true;
                }
                FileList.Add(item);
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
                var item = list[a];
                item.Add = this;
                FileList.Add(item);
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
        var type = _sourceTypeList[DownloadSource];
        if (Set)
        {
            if (type == SourceType.CurseForge)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as CurseForgeModObj.DataObj);
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

        var last = _last!;
        IsDownload = true;
        if (last != null)
        {
            last.NowDownload = true;
        }

        VersionDisplay = false;
        bool res = false;

        //数据包
        if (_now == FileType.DataPacks)
        {
            //选择存档
            var list = await GameBinding.GetWorldsAsync(Obj);
            if (list.Count == 0)
            {
                Model.Show(App.Lang("AddWindow.Error6"));
                IsDownload = false;
                return;
            }

            var world = new List<string>();
            list.ForEach(item => world.Add(item.LevelName));
            var res1 = await Model.Combo(App.Lang("AddWindow.Info7"), world);
            if (res1.Cancel)
            {
                IsDownload = false;
                return;
            }
            var item = list[res1.Index];

            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await WebBinding.Download(item,
                        data.Data as CurseForgeModObj.DataObj),
                    SourceType.Modrinth => await WebBinding.Download(item,
                        data.Data as ModrinthVersionObj),
                    _ => false
                };
                IsDownload = false;
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
                    SourceType.CurseForge => await WebBinding.GetDownloadModList(Obj,
                    data.Data as CurseForgeModObj.DataObj),
                    SourceType.Modrinth => await WebBinding.GetDownloadModList(Obj,
                    data.Data as ModrinthVersionObj),
                    _ => null
                };
                if (list == null)
                {
                    Model.Show(App.Lang("AddWindow.Error9"));
                    IsDownload = false;
                    return;
                }

                if (list.List!.Count == 0)
                {
                    res = await WebBinding.DownloadMod(Obj,
                    [
                        new()
                        {
                            Item = list.Item!,
                            Info = list.Info!,
                            Old = await Obj.ReadMod(mod)
                        }
                    ]);
                    IsDownload = false;
                }
                else
                {
                    //添加模组信息
                    _modList.Clear();
                    _modList.AddRange(list.List);
                    _modsave = new()
                    {
                        Item = list.Item!,
                        Info = list.Info!,
                        Old = await Obj.ReadMod(mod)
                    };
                    OpenModDownloadDisplay();
                    _modList.ForEach(item =>
                    {
                        if (item.Optional == false)
                        {
                            item.Download = true;
                        }
                    });
                    ModsLoad();
                    return;
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
                    SourceType.CurseForge => await WebBinding.Download(_now, Obj,
                        data.Data as CurseForgeModObj.DataObj),
                    SourceType.Modrinth => await WebBinding.Download(_now, Obj,
                        data.Data as ModrinthVersionObj),
                    _ => false
                };
                IsDownload = false;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("AddWindow.Error8"), e);
                res = false;
            }
        }
        if (res)
        {
            Model.Notify(App.Lang("Text.Downloaded"));
            if (last != null)
            {
                last.NowDownload = false;
                last.IsDownload = true;
            }
        }
        else
        {
            if (last != null)
            {
                last.NowDownload = false;
            }
            Model.Show(App.Lang("AddWindow.Error5"));
        }
    }
}
