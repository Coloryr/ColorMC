using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ColorMC.Core.LaunchPath;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Core.Helpers;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ColorMC.Core.Game;
using System.IO;
using ColorMC.Core.Utils;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Downloader;

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
    private ChoiseObj? _choise;


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
        var list1 = new List<GameSettingObj>() { _choise.Game };
        if (_choise.IsGroup)
        {
            if (GameBinding.GetGameGroups().TryGetValue(_choise.Group, out var group))
            {
                list1 = group;
            }
            else
            {
                Model.Show(App.Lang("CollectWindow.Error2"));
                Model.BackClick();
                return;
            }
        }

        foreach (var item in DownloadList)
        {
            if (item.Download == false)
            {
                continue;
            }

            foreach (var item1 in list1)
            {
                var download = item.FileItems[item.SelectVersion];
                var item2 = await MakeDownload(item1, download);
                if (item2 == null)
                {
                    continue;
                }
                list.Add(item2);
            }
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

    private async Task<DownloadItemObj?> MakeDownload(GameSettingObj obj, FileVersionItemModel model)
    {
        ModInfoObj? mod = null;
        if (model.FileType == FileType.Mod && obj.Mods.TryGetValue(model.ID, out mod))
        {
            var res1 = await Model.ShowWait(App.Lang("AddWindow.Info15"));
            if (!res1)
            {
                return null;
            }
        }

        if (model.FileType == FileType.Mod)
        {
            try
            {
                var setting = GameGuiSetting.ReadConfig(obj);
                DownloadModArg? arg = null;
                if (model.SourceType == SourceType.CurseForge)
                {
                    var data = (model.Data as CurseForgeModObj.DataObj)!;
                    arg = new DownloadModArg()
                    {
                        Item = data.MakeModDownloadObj(obj),
                        Info = data.MakeModInfo(InstancesPath.Name11),
                        Old = await obj.ReadMod(mod)
                    };
                }
                else
                {
                    var data = (model.Data as ModrinthVersionObj)!;
                    arg = new DownloadModArg()
                    {
                        Item = data.MakeModDownloadObj(obj),
                        Info = data.MakeModInfo(InstancesPath.Name11),
                        Old = await obj.ReadMod(mod)
                    };
                }

                arg.Item.Later = (s) =>
                {
                    obj.AddModInfo(arg.Info);
                    if (arg.Old is { } old)
                    {
                        PathHelper.Delete(arg.Old.Local);
                    }
                };

                if (arg.Old is { } old && arg.Item.Sha1 != null)
                {
                    if (setting.ModName.TryGetValue(old.Sha1, out var value))
                    {
                        setting.ModName[arg.Item.Sha1] = value;
                        setting.ModName.Remove(old.Sha1);
                    }

                    if (arg.Old.Disable)
                    {
                        arg.Item.Local = Path.ChangeExtension(arg.Item.Local, Mods.Name3);
                        arg.Info.File = Path.ChangeExtension(arg.Info.File, Mods.Name3);
                    }
                }

                GameGuiSetting.WriteConfig(obj, setting);

                return arg.Item;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("AddWindow.Error8"), e);
            }
        }
        else if (model.FileType == FileType.Shaderpack)
        {
            if (model.SourceType == SourceType.CurseForge)
            {
                var data = (model.Data as CurseForgeModObj.DataObj)!;
                return new()
                {
                    Name = data.DisplayName,
                    Url = data.DownloadUrl,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + data.FileName),
                    Sha1 = data.Hashes.Where(a => a.Algo == 1)
                        .Select(a => a.Value).FirstOrDefault(),
                    Overwrite = true
                };
            }
            else
            {
                var data = (model.Data as ModrinthVersionObj)!;
                var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
                return new()
                {
                    Name = data.Name,
                    Url = file.Url,
                    Local = Path.GetFullPath(obj.GetShaderpacksPath() + "/" + file.Filename),
                    Sha1 = file.Hashes.Sha1,
                    Overwrite = true
                };
            }
        }
        else if (model.FileType == FileType.Resourcepack)
        {
            if (model.SourceType == SourceType.CurseForge)
            {
                var data = (model.Data as CurseForgeModObj.DataObj)!;
                return new()
                {
                    Name = data.DisplayName,
                    Url = data.DownloadUrl,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + data.FileName),
                    Sha1 = data.Hashes.Where(a => a.Algo == 1)
                        .Select(a => a.Value).FirstOrDefault(),
                    Overwrite = true
                };
            }
            else
            {
                var data = (model.Data as ModrinthVersionObj)!;
                var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
                return new()
                {
                    Name = data.Name,
                    Url = file.Url,
                    Local = Path.GetFullPath(obj.GetResourcepacksPath() + "/" + file.Filename),
                    Sha1 = file.Hashes.Sha1,
                    Overwrite = true
                };
            }
        }

        return null;
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

    public void Install(CollectItemModel model)
    {
        foreach (var item in CollectList)
        {
            item.IsCheck = false;
        }

        model.IsCheck = true;
        Install();
    }

    private record ChoiseObj
    {
        public bool IsGroup;
        public GameSettingObj Game;
        public string Group;
    }

    public async void Install()
    {
        if (!HaveSelect())
        {
            return;
        }

        var items = new List<string>();
        var items1 = new List<ChoiseObj>();

        foreach (var item in GameBinding.GetGameGroups())
        {
            items1.Add(new()
            { 
                IsGroup = true,
                Group = item.Key
            });

            items.Add(string.Format(App.Lang("CollectWindow.Info7"), item.Key));

            foreach (var item1 in item.Value)
            {
                items1.Add(new()
                {
                    IsGroup = false,
                    Game = item1
                });

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
                var item1 = _choise.IsGroup ? await WebBinding.GetFileList(item.Obj.Source, item.Obj.Pid, 0, null, Loaders.Normal, item.Obj.FileType)
                : await WebBinding.GetFileList(item.Obj.Source, item.Obj.Pid, 0, _choise.Game.Version, _choise.Game.Loader, item.Obj.FileType);
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

    public bool HaveSelect()
    {
        return CollectList.Any(item => item.IsCheck);
    }
}
