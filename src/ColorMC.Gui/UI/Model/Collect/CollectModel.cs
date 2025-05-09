using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Collect;

/// <summary>
/// 收藏界面
/// </summary>
public partial class CollectModel : TopModel, ICollectControl
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
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<FileModVersionModel> DownloadList { get; init; } = [];

    /// <summary>
    /// 收藏列表
    /// </summary>
    private readonly Dictionary<string, CollectItemModel> _list = [];

    /// <summary>
    /// 是否显示模组
    /// </summary>
    [ObservableProperty]
    private bool _mod;
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
    /// 是否在下载中
    /// </summary>
    [ObservableProperty]
    private bool _isDownload;
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
    /// 选中的收藏
    /// </summary>
    private CollectItemModel? _select;
    /// <summary>
    /// 选中的游戏实例
    /// </summary>
    private GameSettingObj? _choise;

    public CollectModel(BaseModel model) : base(model)
    {
        var conf = CollectUtils.Collect;

        _mod = conf.Mod;
        _resourcepack = conf.ResourcePack;
        _shaderpack = conf.Shaderpack;

        Groups.Add("");

        foreach (var item in conf.Groups)
        {
            Groups.Add(item.Key);
        }

        LoadItems();
        Load();
    }

    /// <summary>
    /// 模组选中
    /// </summary>
    /// <param name="value"></param>
    partial void OnModChanged(bool value)
    {
        CollectUtils.Setting(Mod, Resourcepack, Shaderpack);

        Load();
    }

    /// <summary>
    /// 资源包选中
    /// </summary>
    /// <param name="value"></param>
    partial void OnResourcepackChanged(bool value)
    {
        CollectUtils.Setting(Mod, Resourcepack, Shaderpack);

        Load();
    }

    /// <summary>
    /// 光影包选中
    /// </summary>
    /// <param name="value"></param>
    partial void OnShaderpackChanged(bool value)
    {
        CollectUtils.Setting(Mod, Resourcepack, Shaderpack);

        Load();
    }

    /// <summary>
    /// 分组修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnGroupChanged(string value)
    {
        GroupDelete = !string.IsNullOrWhiteSpace(value);

        Load();
    }

    /// <summary>
    /// 下载所有选中的收藏
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadAll()
    {
        if (_choise == null)
        {
            return;
        }

        if (GameBinding.GetGame(_choise.UUID) == null)
        {
            Model.Show(App.Lang("CollectWindow.Error2"));
            Model.BackClick();
            return;
        }

        var list = new ConcurrentBag<FileItemObj>();

        //获取下载项目
        await Parallel.ForEachAsync(DownloadList, async (item, cancel) =>
        {
            if (item.Download == false)
            {
                return;
            }

            var download = item.FileItems[item.SelectVersion];
            var item2 = await WebBinding.MakeDownload(_choise, download, Model);
            if (item2 == null)
            {
                return;
            }
            list.Add(item2);
        });

        //开始下载
        Model.Progress(App.Lang("CollectWindow.Info10"));
        var res = await DownloadManager.StartAsync([.. list]);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("CollectWindow.Error3"));
        }
        else
        {
            Model.Notify(App.Lang("CollectWindow.Info11"));
        }
    }

    /// <summary>
    /// 添加收藏分组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGroup()
    {
        var res = await Model.InputWithEditAsync(App.Lang("CollectWindow.Info1"), false);
        if (res.Cancel || string.IsNullOrWhiteSpace(res.Text1))
        {
            return;
        }
        if (CollectUtils.Collect.Groups.ContainsKey(res.Text1))
        {
            Model.Show(App.Lang("CollectWindow.Error1"));
            return;
        }
        CollectUtils.AddGroup(res.Text1);
        Groups.Add(res.Text1);
        Group = res.Text1;
    }

    /// <summary>
    /// 删除收藏分组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DeleteGroup()
    {
        var res = await Model.ShowAsync(App.Lang("CollectWindow.Info4"));
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
        if (string.IsNullOrWhiteSpace(Group))
        {
            var res = await Model.ShowAsync(App.Lang("CollectWindow.Info2"));
            if (res == true)
            {
                CollectUtils.Clear();
                Close();
                EmptyDisplay = true;
                HaveChoise = false;
            }
        }
        else
        {
            var res = await Model.ShowAsync(App.Lang("CollectWindow.Info3"));
            if (res == true)
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
        if (!HaveSelect())
        {
            return;
        }

        //获取游戏实例
        var items = new List<string>();
        var items1 = new List<GameSettingObj>();

        foreach (var item in GameBinding.GetGameGroups())
        {
            foreach (var item1 in item.Value)
            {
                items1.Add(item1);
                items.Add(item1.Name);
            }
        }

        //选择一个游戏实例
        var res = await Model.Combo(App.Lang("CollectWindow.Info6"), items);
        if (res.Cancel)
        {
            return;
        }

        _choise = items1[res.Index];

        Model.Progress(App.Lang("CollectWindow.Info9"));

        var list = new ConcurrentBag<FileModVersionModel>();

        DownloadList.Clear();

        await Parallel.ForEachAsync(CollectList, async (item, cancel) =>
        {
            if (item.IsCheck)
            {
                var item1 = await WebBinding.GetFileList(item.Obj.Source, item.Obj.Pid, 0, _choise.Version, _choise.Loader, item.Obj.FileType);
                if (item1.Count == 0)
                {
                    return;
                }
                var model1 = new FileModVersionModel(item.Name, item1.List!);

                list.Add(model1);
            }
        });

        DownloadList.AddRange(list);

        Model.ProgressClose();
        Model.PushBack(() =>
        {
            DownloadList.Clear();
            IsDownload = false;
            _choise = null;
        });
        IsDownload = true;
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

        if (string.IsNullOrWhiteSpace(Group))
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

    protected override void MinModeChange()
    {
        foreach (var item in _list)
        {
            item.Value.SetMin(MinMode);
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
        if (_select != null)
        {
            _select.IsSelect = false;
        }
        _select = item;
        item.IsSelect = true;
    }

    /// <summary>
    /// 勾选收藏
    /// </summary>
    public void ChoiseChange()
    {
        HaveChoise = CollectList.Any(item => item.IsCheck);
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
    public bool HaveSelect()
    {
        HaveChoise = CollectList.Any(item => item.IsCheck);
        return HaveChoise;
    }

    /// <summary>
    /// 是否选择收藏分组
    /// </summary>
    /// <returns></returns>
    public bool HaveGroup()
    {
        return !string.IsNullOrWhiteSpace(Group);
    }

    /// <summary>
    /// 删除选择的收藏
    /// </summary>
    public async void DeleteSelect()
    {
        if (!HaveSelect())
        {
            return;
        }

        var res = await Model.ShowAsync(App.Lang("CollectWindow.Info12"));
        if (!res)
        {
            return;
        }

        var list = new List<string>();

        foreach (var item in CollectList)
        {
            if (item.IsCheck)
            {
                list.Add(item.Obj.UUID);
            }
        }

        CollectUtils.RemoveItem(Group, list);
    }

    /// <summary>
    /// 将手册添加到其他收藏分组
    /// </summary>
    public async void GroupSelect()
    {
        if (!HaveSelect())
        {
            return;
        }

        var list = new List<string>(CollectUtils.Collect.Groups.Keys);
        list.Remove(Group);

        var res = await Model.Combo(App.Lang("CollectWindow.Info13"), list);
        if (res.Cancel)
        {
            return;
        }

        list.Clear();

        foreach (var item in CollectList)
        {
            if (item.IsCheck)
            {
                list.Add(item.Obj.UUID);
            }
        }

        CollectUtils.AddItem(Group, list);
    }

    /// <summary>
    /// 安装所选的收藏
    /// </summary>
    public async void Install()
    {
        await InstallSelect();
    }
}
