using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Collect;

public partial class CollectModel : TopModel, ICollectWindow
{
    public ObservableCollection<CollectItemModel> CollectList { get; init; } = [];
    public ObservableCollection<string> Groups { get; init; } = [];

    /// <summary>
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<FileModVersionModel> DownloadList { get; init; } = [];

    private readonly Dictionary<string, CollectItemModel> _list = [];

    [ObservableProperty]
    private bool _mod;
    [ObservableProperty]
    private bool _resourcepack;
    [ObservableProperty]
    private bool _shaderpack;

    [ObservableProperty]
    private bool _groupDelete;
    [ObservableProperty]
    private bool _emptyDisplay;
    [ObservableProperty]
    private bool _isDownload;

    [ObservableProperty]
    private string _group;

    private CollectItemModel? _select;
    private GameSettingObj? _choise;

    private readonly string _useName;

    public CollectModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "CollectModel";

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

    partial void OnModChanged(bool value)
    {
        CollectUtils.Setting(Mod, Resourcepack, Shaderpack);

        Load();
    }

    partial void OnResourcepackChanged(bool value)
    {
        CollectUtils.Setting(Mod, Resourcepack, Shaderpack);

        Load();
    }

    partial void OnShaderpackChanged(bool value)
    {
        CollectUtils.Setting(Mod, Resourcepack, Shaderpack);

        Load();
    }

    partial void OnGroupChanged(string value)
    {
        GroupDelete = !string.IsNullOrWhiteSpace(value);

        Load();
    }

    [RelayCommand]
    public async Task DownloadAll()
    {
        if (_choise == null)
        {
            return;
        }

        var list = new List<DownloadItemObj>();
        if (GameBinding.GetGame(_choise.UUID) == null)
        {
            Model.Show(App.Lang("CollectWindow.Error2"));
            Model.BackClick();
            return;
        }

        foreach (var item in DownloadList)
        {
            if (item.Download == false)
            {
                continue;
            }

            var download = item.FileItems[item.SelectVersion];
            var item2 = await WebBinding.MakeDownload(_choise, download, Model);
            if (item2 == null)
            {
                continue;
            }
            list.Add(item2);
        }

        Model.Progress(App.Lang("CollectWindow.Info10"));
        var res = await DownloadManager.StartAsync(list);
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

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("CollectWindow.Info1"), false);
        if (Cancel || string.IsNullOrWhiteSpace(Text))
        {
            return;
        }
        if (CollectUtils.Collect.Groups.ContainsKey(Text))
        {
            Model.Show(App.Lang("CollectWindow.Error1"));
            return;
        }
        CollectUtils.AddGroup(Text);
        Groups.Add(Text);
        Group = Text;
    }

    [RelayCommand]
    public async Task DeleteGroup()
    {
        var res = await Model.ShowWait(App.Lang("CollectWindow.Info4"));
        if (!res)
        {
            return;
        }
        CollectUtils.DeleteGroup(Group);
        Groups.Remove(Group);
    }

    [RelayCommand]
    public async Task ClearGroup()
    {
        if (string.IsNullOrWhiteSpace(Group))
        {
            var res = await Model.ShowWait(App.Lang("CollectWindow.Info2"));
            if (res == true)
            {
                CollectUtils.Clear();
                Close();
                EmptyDisplay = true;
            }
        }
        else
        {
            var res = await Model.ShowWait(App.Lang("CollectWindow.Info3"));
            if (res == true)
            {
                CollectUtils.Clear(Group);
                Load();
            }
        }
    }

    [RelayCommand]
    public async Task InstallSelect()
    {
        if (!HaveSelect())
        {
            return;
        }

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

        var res = await Model.ShowCombo(App.Lang("CollectWindow.Info6"), items);
        if (res.Cancel)
        {
            return;
        }

        _choise = items1[res.Index];

        Model.Progress(App.Lang("CollectWindow.Info9"));

        DownloadList.Clear();

        foreach (var item in CollectList)
        {
            if (item.IsCheck)
            {
                var item1 = await WebBinding.GetFileList(item.Obj.Source, item.Obj.Pid, 0, _choise.Version, _choise.Loader, item.Obj.FileType);
                if (item1.Count == 0)
                {
                    continue;
                }
                var model1 = new FileModVersionModel(item.Name, item1.List!);

                DownloadList.Add(model1);
            }
        }

        Model.ProgressClose();
        Model.PushBack(() =>
        {
            DownloadList.Clear();
            IsDownload = false;
            _choise = null;
        });
        IsDownload = true;
    }

    private void Load()
    {
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

    public void SetSelect(CollectItemModel item)
    {
        if (_select != null)
        {
            _select.IsSelect = false;
        }
        _select = item;
        item.IsSelect = true;
    }

    public async void Install(CollectItemModel model)
    {
        foreach (var item in CollectList)
        {
            item.IsCheck = false;
        }

        model.IsCheck = true;
        await InstallSelect();
    }

    public bool HaveSelect()
    {
        return CollectList.Any(item => item.IsCheck);
    }

    public bool HaveGroup()
    {
        return !string.IsNullOrWhiteSpace(Group);
    }

    public async void DeleteSelect()
    {
        if (!HaveSelect())
        {
            return;
        }

        var res = await Model.ShowWait(App.Lang("CollectWindow.Info12"));
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

    public async void GroupSelect()
    {
        if (!HaveSelect())
        {
            return;
        }

        var list = new List<string>(CollectUtils.Collect.Groups.Keys);
        list.Remove(Group);

        var res = await Model.ShowCombo(App.Lang("CollectWindow.Info13"), list);
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

    public async void Install()
    {
        await InstallSelect();
    }
}
