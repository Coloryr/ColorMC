using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Collect;

/// <summary>
/// 收藏界面
/// </summary>
public partial class CollectModel : ControlModel, ICollectControl
{
    /// <summary>
    /// 收藏项目
    /// </summary>
    public ObservableCollection<CollectItemModel> CollectList { get; init; } = [];
    /// <summary>
    /// 收藏分组
    /// </summary>
    public ObservableCollection<string> Groups { get; init; } = [];

    /// <summary>
    /// 收藏列表
    /// </summary>
    private readonly Dictionary<string, CollectItemModel> _list = [];

    /// <summary>
    /// 正在下载的项目
    /// </summary>
    public ObservableCollection<FileItemDownloadModel> NowDownload { get; init; } = [];

    /// <summary>
    /// 是否显示模组
    /// </summary>
    [ObservableProperty]
    private bool _mod;
    /// <summary>
    /// 是否显示整合包
    /// </summary>
    [ObservableProperty]
    private bool _modpack;
    /// <summary>
    /// 是否显示资源包
    /// </summary>
    [ObservableProperty]
    private bool _resourcepack;
    /// <summary>
    /// 是否显示光影包
    /// </summary>
    [ObservableProperty]
    private bool _shaderpack;

    /// <summary>
    /// 是否允许删除分组
    /// </summary>
    [ObservableProperty]
    private bool _groupDelete;
    /// <summary>
    /// 是否没有收藏内容
    /// </summary>
    [ObservableProperty]
    private bool _emptyDisplay;

    /// <summary>
    /// 是否选中项目
    /// </summary>
    [ObservableProperty]
    private bool _haveChoise;

    /// <summary>
    /// 选中的收藏分组
    /// </summary>
    [ObservableProperty]
    private string _group;
    /// <summary>
    /// 选中的分组序号
    /// </summary>
    [ObservableProperty]
    private int _index;

    /// <summary>
    /// 下载文本
    /// </summary>
    [ObservableProperty]
    private string? _downloadText;
    /// <summary>
    /// 是否显示下载文本
    /// </summary>
    [ObservableProperty]
    private string? _displayText;

    /// <summary>
    /// 是否显示下载列表
    /// </summary>
    [ObservableProperty]
    private bool _displayDownload = false;
    /// <summary>
    /// 是否有下载项目
    /// </summary>
    [ObservableProperty]
    private bool _haveDownload = false;

    /// <summary>
    /// 选中的收藏
    /// </summary>
    private CollectItemModel? _select;

    private bool _isDisplayRun;
    private int _displayRunStage;

    private static readonly string[] s_displayStage = [".    ", "..   ", "...  ", ".... ", "....."];

    public CollectModel(WindowModel model) : base(model)
    {
        var conf = CollectUtils.Collect;

        _mod = conf.Mod;
        _modpack = conf.Modpack;
        _resourcepack = conf.ResourcePack;
        _shaderpack = conf.Shaderpack;

        EventManager.ModpackInstall += EventManager_ModpackInstall;
        EventManager.ModpackStop += EventManager_ModpackStop;

        Groups.Add(LangUtils.Get("CollectWindow.Text26"));

        foreach (var item in conf.Groups)
        {
            Groups.Add(item.Key);
        }

        Index = 0;

        LoadItems();
        Load();
    }

    partial void OnModpackChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    /// <summary>
    /// 模组选中
    /// </summary>
    /// <param name="value"></param>
    partial void OnModChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    /// <summary>
    /// 资源包选中
    /// </summary>
    /// <param name="value"></param>
    partial void OnResourcepackChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    /// <summary>
    /// 光影包选中
    /// </summary>
    /// <param name="value"></param>
    partial void OnShaderpackChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    /// <summary>
    /// 分组修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnIndexChanged(int value)
    {
        if (value == -1)
        {
            Index = 0;
            return;
        }

        GroupDelete = Index != 0;
    }

    partial void OnGroupChanged(string value)
    {
        if (Index < 0)
        {
            return;
        }

        Load();
    }

    /// <summary>
    /// 添加收藏分组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGroup()
    {
        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LangUtils.Get("CollectWindow.Text7")
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true || string.IsNullOrWhiteSpace(dialog.Text1))
        {
            return;
        }
        if (CollectUtils.Collect.Groups.ContainsKey(dialog.Text1))
        {
            Window.Show(LangUtils.Get("CollectWindow.Text19"));
            return;
        }
        CollectUtils.AddGroup(dialog.Text1);
        Groups.Add(dialog.Text1);
        Group = dialog.Text1;
    }

    /// <summary>
    /// 删除收藏分组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DeleteGroup()
    {
        if (Group == null || Index == 0)
        {
            return;
        }
        var res = await Window.ShowChoice(LangUtils.Get("CollectWindow.Text10"));
        if (!res)
        {
            return;
        }
        CollectUtils.DeleteGroup(Group);
        Groups.Remove(Group);
    }

    /// <summary>
    /// 清空收藏分组内容
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task ClearGroup()
    {
        if (Index == 0)
        {
            var res = await Window.ShowChoice(LangUtils.Get("CollectWindow.Text8"));
            if (res)
            {
                CollectUtils.Clear();
                Close();
                EmptyDisplay = true;
                HaveChoise = false;
            }
        }
        else
        {
            var res = await Window.ShowChoice(LangUtils.Get("CollectWindow.Text9"));
            if (res)
            {
                CollectUtils.Clear(Group);
                Load();
                HaveChoise = false;
            }
        }
    }

    /// <summary>
    /// 安装所选
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task InstallSelect()
    {
        HaveSelect(out var Modpack, out var other);

        if (Modpack && other)
        {
            Window.Show(LangUtils.Get("CollectWindow.Text25"));
            return;
        }

        if (other)
        {
            //选择一个游戏实例
            var dialog = new SelectGameModel(Window.WindowId, true)
            {
                Text = LangUtils.Get("CollectWindow.Text12")
            };
            var res = await Window.ShowDialogWait(dialog);
            if (res is not true || dialog.Select == null)
            {
                return;
            }

            var game = InstancesPath.GetGame(dialog.Select.UUID);

            if (game == null)
            {
                Window.Show(LangUtils.Get("CollectWindow.Text29"));
                return;
            }

            var dialog1 = Window.ShowProgress(LangUtils.Get("CollectWindow.Text14"));

            var list = new ConcurrentBag<FileModVersionModel>();

            bool error = false;

            await Parallel.ForEachAsync(CollectList, async (item, cancel) =>
            {
                if (!item.IsCheck || error)
                {
                    return;
                }

                var item1 = await WebBinding.GetFileListAsync(item.Obj.Source,
                        item.Obj.Pid, 0, game.Version, game.Loader, item.Obj.FileType);
                if (item1.Count == 0)
                {
                    error = true;
                    return;
                }
                var model1 = new FileModVersionModel(item.Name, item1.List!)
                {
                    Download = true
                };
                list.Add(model1);
            });

            Window.CloseDialog(dialog1);

            if (error)
            {
                Window.Show(LangUtils.Get("CollectWindow.Text30"));
                return;
            }

            var dialog2 = new CollectDownloadModel(Window.WindowId);

            dialog2.DownloadList.AddRange(list);

            var res1 = await Window.ShowDialogWait(dialog2);
            if (res1 is not true)
            {
                return;
            }

            if (InstancesPath.GetGame(game.UUID) == null)
            {
                Window.Show(LangUtils.Get("CollectWindow.Text20"));
                return;
            }

            var list1 = new ConcurrentBag<FileItemObj>();

            error = false;
            //获取下载项目
            await Parallel.ForEachAsync(dialog2.DownloadList, async (item, cancel) =>
            {
                if (!item.Download || error)
                {
                    return;
                }

                var download = item.FileItems[item.SelectVersion];
                var item2 = await WebBinding.MakeDownloadAsync(game, download, Window);
                if (item2 == null)
                {
                    error = true;
                    return;
                }
                list1.Add(item2);
            });

            if (error)
            {
                Window.Show(LangUtils.Get("CollectWindow.Text30"));
                return;
            }

            //开始下载
            dialog1 = Window.ShowProgress(LangUtils.Get("CollectWindow.Text15"));
            var res2 = await DownloadManager.StartAsync([.. list1]);
            Window.CloseDialog(dialog1);
            if (!res2)
            {
                Window.Show(LangUtils.Get("CollectWindow.Text21"));
            }
            else
            {
                Window.Notify(LangUtils.Get("CollectWindow.Text16"));
            }
        }
        else if (Modpack)
        {
            var dialog1 = Window.ShowProgress(LangUtils.Get("CollectWindow.Text14"));

            var list = new ConcurrentBag<FileModVersionModel>();
            var dir = new Dictionary<string, FileItemModel>();

            bool error = false;

            await Parallel.ForEachAsync(CollectList, async (item, cancel) =>
            {
                if (!item.IsCheck || error)
                {
                    return;
                }

                var project = await WebBinding.GetModpackAsync(item.Obj.Source, item.Obj.Pid);
                if (project == null)
                {
                    error = true;
                    return;
                }
                dir[item.Obj.Pid] = project;
                var item1 = await WebBinding.GetFileListAsync(item.Obj.Source, item.Obj.Pid,
                    0, null, Loaders.Normal, item.Obj.FileType);
                if (item1.Count == 0)
                {
                    error = true;
                    return;
                }
                var model1 = new FileModVersionModel(item.Name, item1.List!)
                {
                    Download = true
                };

                list.Add(model1);
            });

            Window.CloseDialog(dialog1);

            if (error)
            {
                Window.Show(LangUtils.Get("CollectWindow.Text30"));
                return;
            }

            var dialog2 = new CollectDownloadModel(Window.WindowId);

            dialog2.DownloadList.AddRange(list);

            var res1 = await Window.ShowDialogWait(dialog2);
            if (res1 is not true)
            {
                return;
            }

            var dialog = Window.ShowProgress(LangUtils.Get("CollectWindow.Text15"));

            var gui = new OverGameGui(Window);
            var pack = new TopModPackGui(dialog);

            var count = dialog2.DownloadList.Where(item => item.Download).Count();

            foreach (var item in dialog2.DownloadList)
            {
                if (!item.Download)
                {
                    continue;
                }

                var data = item.FileItems[item.SelectVersion];

                FileItemDownloadModel info;

                var res = new GameRes();
                if (data.Obj.Source == SourceType.CurseForge)
                {
                    var data1 = (data.Data as CurseForgeModObj.CurseForgeDataObj)!;
                    info = new FileItemDownloadModel
                    {
                        Window = Window,
                        Name = data1.DisplayName,
                        Obj = data.Obj
                    };
                    StartDownload(info);
                    GameManager.StartDownload(info);
                    res = await AddGameHelper.InstallCurseForge(null, data1, dir[data.Obj.Pid].Logo, gui, pack);
                    StopDownload(info);
                    GameManager.StopDownload(info, res.State);
                }
                else if (data.Obj.Source == SourceType.Modrinth)
                {
                    var data1 = (data.Data as ModrinthVersionObj)!;
                    info = new FileItemDownloadModel
                    {
                        Window = Window,
                        Name = data1.Name,
                        Obj = data.Obj
                    };
                    StartDownload(info);
                    GameManager.StartDownload(info);
                    res = await AddGameHelper.InstallModrinth(null, data1, dir[data.Obj.Pid].Logo, gui, pack);
                    StopDownload(info);
                    GameManager.StopDownload(info, res.State);
                }

                if (!res.State && count > 0)
                {
                    var res2 = await Window.ShowChoice(LangUtils.Get("CollectWindow.Text31"));
                    if (!res2)
                    {
                        break;
                    }
                }

                count--;
            }

            pack.Stop();
        }
    }

    /// <summary>
    /// 显示收藏列表
    /// </summary>
    private void Load()
    {
        HaveChoise = false;
        _select = null;

        foreach (var item in CollectList)
        {
            item.Uncheck();
        }

        CollectList.Clear();

        if (Index == 0)
        {
            foreach (var item in _list.Values)
            {
                if (item.Obj.FileType == FileType.Mod && Mod)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Resourcepack && Resourcepack)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Shaderpack && Shaderpack)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Modpack && Modpack)
                {
                    CollectList.Add(item);
                }
            }
        }
        else if (CollectUtils.Collect.Groups.TryGetValue(Group, out var group))
        {
            foreach (var uuid in group)
            {
                if (!_list.TryGetValue(uuid, out var item))
                {
                    continue;
                }
                if (item.Obj.FileType == FileType.Mod && Mod)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Resourcepack && Resourcepack)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Shaderpack && Shaderpack)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Modpack && Modpack)
                {
                    CollectList.Add(item);
                }
            }
        }

        EmptyDisplay = CollectList.Count == 0;
    }

    /// <summary>
    /// 加载收藏列表
    /// </summary>
    private void LoadItems()
    {
        foreach (var item in CollectUtils.Collect.Items)
        {
            _list.Add(item.Key, new CollectItemModel(item.Value)
            {
                Add = this
            });
        }
    }

    public override void Close()
    {
        CollectList.Clear();
        foreach (var item in _list.Values)
        {
            item.Close();
        }

        _list.Clear();
    }

    /// <summary>
    /// 更新收藏列表
    /// </summary>
    public void Update()
    {
        CollectList.Clear();

        var newlist = new List<CollectItemObj>();
        var deletelist = new List<CollectItemModel>();

        foreach (var item in CollectUtils.Collect.Items)
        {
            if (!_list.ContainsKey(item.Key))
            {
                newlist.Add(item.Value);
            }
        }

        foreach (var item in _list)
        {
            if (!CollectUtils.Collect.Items.ContainsKey(item.Key))
            {
                deletelist.Add(item.Value);
            }
        }

        foreach (var item in newlist)
        {
            _list.Add(item.UUID, new(item)
            {
                Add = this
            });
        }

        foreach (var item in deletelist)
        {
            item.Close();
            _list.Remove(item.Obj.UUID);
        }

        Load();
    }

    /// <summary>
    /// 选中一个收藏
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(CollectItemModel item)
    {
        _select?.IsSelect = false;
        _select = item;
        item.IsSelect = true;
    }

    /// <summary>
    /// 勾选收藏
    /// </summary>
    public void ChoiseChange()
    {
        HaveSelect(out var Modpack, out var other);
        if (other && Modpack)
        {
            HaveChoise = false;
        }
        else if (other || Modpack)
        {
            HaveChoise = true;
        }
        else
        {
            HaveChoise = false;
        }
    }

    /// <summary>
    /// 安装指定收藏
    /// </summary>
    /// <param name="model"></param>
    public async void Install(CollectItemModel model)
    {
        foreach (var item in CollectList)
        {
            item.IsCheck = false;
        }

        model.IsCheck = true;
        await InstallSelect();
    }

    /// <summary>
    /// 是否有选择收藏
    /// </summary>
    /// <returns></returns>
    public void HaveSelect(out bool Modpack, out bool other)
    {
        Modpack = false;
        other = false;

        foreach (var item in CollectList)
        {
            if (!item.IsCheck)
            {
                continue;
            }
            if (item.Obj.FileType == FileType.Modpack)
            {
                Modpack = true;
            }
            else
            {
                other = true;
            }
        }
    }

    /// <summary>
    /// 删除选择的收藏
    /// </summary>
    public async void DeleteSelect()
    {
        HaveSelect(out var Modpack, out var other);
        if (!Modpack && !other)
        {
            return;
        }

        var list = CollectList.Where(item => item.IsCheck).Select(item => item.Obj.UUID);

        if (Index == 0)
        {
            var res = await Window.ShowChoice(LangUtils.Get("CollectWindow.Text17"));
            if (!res)
            {
                return;
            }

            foreach (var item in list)
            {
                CollectUtils.RemoveItem(item);
            }
        }
        else
        {
            var res = await Window.ShowChoice(LangUtils.Get("CollectWindow.Text28"));
            if (!res)
            {
                return;
            }

            CollectUtils.RemoveItem(Group, list);
        }

        Load();
    }

    /// <summary>
    /// 添加到其他收藏分组
    /// </summary>
    public async void GroupSelect()
    {
        HaveSelect(out var Modpack, out var other);
        if (!Modpack && !other)
        {
            return;
        }

        var list = new List<string>(CollectUtils.Collect.Groups.Keys);
        if (Index != 0)
        {
            list.Remove(Group);
        }

        var dialog = new SelectModel(Window.WindowId)
        {
            Text = LangUtils.Get("CollectWindow.Text18"),
            Items = [.. list]
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }

        var group = list[dialog.Index];

        list.Clear();

        foreach (var item in CollectList)
        {
            if (item.IsCheck)
            {
                list.Add(item.Obj.UUID);
            }
        }

        CollectUtils.AddItem(group, list);
    }

    /// <summary>
    /// 安装所选的收藏
    /// </summary>
    public async void Install()
    {
        await InstallSelect();
    }

    public bool HaveSelect()
    {
        HaveSelect(out var Modpack, out var other);
        return Modpack || other;
    }

    public bool CanDownload()
    {
        return HaveChoise;
    }

    /// <summary>
    /// 开始下载项目
    /// </summary>
    /// <param name="info">下载项目</param>
    public void StartDownload(FileItemDownloadModel info)
    {
        NowDownload.Add(info);

        DownloadReload();
    }

    /// <summary>
    /// 下载项目结束
    /// </summary>
    /// <param name="info">下载项目</param>
    /// <param name="done">是否下载完成</param>
    public void StopDownload(FileItemDownloadModel info)
    {
        NowDownload.Remove(info);

        DownloadReload();
    }

    private void DownloadReload()
    {
        HaveDownload = NowDownload.Any();
        if (HaveDownload)
        {
            StartDisplay();
            DownloadText = string.Format(LangUtils.Get("AddModPackWindow.Text41"), NowDownload.Count);
        }
        else
        {
            DownloadText = LangUtils.Get("AddModPackWindow.Text45");
            _isDisplayRun = false;
        }
    }

    private void StartDisplay()
    {
        _isDisplayRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromSeconds(0.5));
    }

    private bool Run()
    {
        DisplayText = s_displayStage[_displayRunStage];
        _displayRunStage++;
        _displayRunStage %= s_displayStage.Length;
        return _isDisplayRun;
    }

    private void EventManager_ModpackStop(SourceItemObj obj, bool res)
    {
        foreach (var item in _list.Values)
        {
            if (obj.CheckProject(item.Obj.Source, item.Obj.FileType, item.Obj.Pid))
            {
                item.IsDownload = false;
            }
        }
    }

    private void EventManager_ModpackInstall(SourceItemObj obj)
    {
        foreach (var item in _list.Values)
        {
            if (obj.CheckProject(item.Obj.Source, item.Obj.FileType, item.Obj.Pid))
            {
                item.IsDownload = true;
            }
        }
    }
}
