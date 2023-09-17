using Avalonia.Controls;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameCloud;

public partial class GameCloudModel : GameModel
{
    public List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/GameExport/item1.svg",
            Text = App.GetLanguage("GameCloudWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/GameExport/item2.svg",
            Text = App.GetLanguage("GameCloudWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/GameExport/item4.svg",
            Text = App.GetLanguage("GameCloudWindow.Tabs.Text3") }
    };

    [ObservableProperty]
    private bool _displayFilter = true;

    [ObservableProperty]
    private int _nowView;

    [ObservableProperty]
    private string _title;

    /// <summary>
    /// 导出的文件列表
    /// </summary>
    private FilesPage _files;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    [ObservableProperty]
    private bool _enable;
    [ObservableProperty]
    private string _configTime;
    [ObservableProperty]
    private string _localConfigTime;

    private bool _configHave;

    private WorldCloudModel? _selectWorld;

    public ObservableCollection<WorldCloudModel> WorldCloudList { get; } = new();

    public string UUID => Obj.UUID;

    public GameCloudModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {
        _title = TabItems[0].Text;

        LoadWorld();
    }

    partial void OnNowViewChanged(int value)
    {
        CloseSide();

        Title = TabItems[NowView].Text;
    }

    [RelayCommand]
    public void OpenSide()
    {
        OnPropertyChanged("SideOpen");
    }

    [RelayCommand]
    public void CloseSide()
    {
        OnPropertyChanged("SideClose");
    }

    [RelayCommand]
    public async Task MakeEnable()
    {
        if (Enable)
        {
            return;
        }

        Model.Progress("启用云同步中");
        var res = await WebBinding.StartCloud(Obj);
        Model.ProgressClose();
        if (res == null)
        {
            Model.ShowOk("云服务器错误", WindowClose);
            return;
        }
        if (res.Value == AddSaveState.Exist)
        {
            Model.Show("游戏实例已经启用同步了");
            return;
        }
        else if (res.Value == AddSaveState.Error)
        {
            Model.Show("游戏实例启用同步错误");
            return;
        }

        Model.Notify("同步已启用");
        Enable = true;
    }

    [RelayCommand]
    public async Task MakeDisable()
    {
        if (!Enable)
        {
            return;
        }

        var ok = await Model.ShowWait("关闭云同步会删除服务器上的所有东西，是否继续");
        if (!ok)
        {
            return;
        }

        Model.Progress("关闭云同步中");
        var res = await WebBinding.StopCloud(Obj);
        Model.ProgressClose();
        if (res == null)
        {
            Model.ShowOk("云服务器错误", WindowClose);
            return;
        }
        if (!res.Value)
        {
            Model.Show("云同步关闭失败");
            return;
        }

        Model.Notify("同步已关闭");
        Enable = false;
    }

    [RelayCommand]
    public async Task UploadConfig()
    {
        Model.Progress("正在打包");
        var files = _files.GetSelectItems();
        var data = GameCloudUtils.GetCloudData(Obj);
        string dir = Obj.GetBasePath();
        data.Config ??= new();
        data.Config.Clear();
        foreach (var item in files)
        {
            data.Config.Add(item[(dir.Length + 1)..]);
        }
        string name = Path.GetFullPath(dir + "/config.zip");
        files.Remove(name);
        await new ZipUtils().ZipFile(name, files, dir);
        Model.ProgressUpdate("上传中");
        await GameCloudUtils.UploadConfig(Obj.UUID, name);
        PathHelper.Delete(name);
        await LoadCloud();
        if (_configHave)
        {
            data.ConfigTime = DateTime.Parse(ConfigTime);
        }
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
        Model.ProgressClose();
    }

    [RelayCommand]
    public async Task DownloadConfig()
    {
        Model.Progress("正在下载");
        var data = GameCloudUtils.GetCloudData(Obj);
        string local = Path.GetFullPath(Obj.GetBasePath() + "/config.zip");
        var res = await GameCloudUtils.DownloadConfig(Obj.UUID, local);
        if (res != true)
        {
            Model.ProgressClose();
            Model.Show("同步失败");
            return;
        }
        Model.ProgressUpdate("解压中");
        await GameBinding.UnZipCloudConfig(Obj, data, local);
        await LoadCloud();
        if (_configHave)
        {
            data.ConfigTime = DateTime.Parse(ConfigTime);
        }
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
        Model.ProgressClose();
    }

    public async Task LoadCloud()
    {
        Model.Progress("检查云同步中");
        var res = await WebBinding.CheckCloud(Obj);
        Model.ProgressClose();
        if (res.Item1 == null)
        {
            Model.ShowOk("云服务器错误", WindowClose);
            return;
        }
        Enable = (bool)res.Item1;
        _configHave = res.Item2 != null;
        ConfigTime = res.Item2 ?? "没有同步";
    }

    public async void Load()
    {
        if (!GameCloudUtils.Connect)
        {
            Model.ShowOk("云服务器未链接", WindowClose);
            return;
        }
        await LoadCloud();

        string dir = Obj.GetBasePath();
        _files = new FilesPage(dir, false);
        var data = GameCloudUtils.GetCloudData(Obj);
        LocalConfigTime = data.ConfigTime.ToString();

        var list = new List<string>()
        {
            Obj.GetGameJsonFile(),
            Obj.GetModInfoJsonFile(),
            Obj.GetIconFile(),
            Obj.GetLaunchFile(),
            Obj.GetModJsonFile(),
            Obj.GetModPackJsonFile()
        };
        _files.SetSelectItems(list);

        list.Clear();
        if (data.Config != null)
        {
            foreach (var item in data.Config)
            {
                list.Add(Path.GetFullPath(dir + "/" + item));
            }
        }
        else
        {
            list.Add(Obj.GetConfigPath());
            list.Add(Obj.GetOptionsFile());
            foreach (var mod in await GameBinding.GetGameMods(Obj))
            {
                if (mod.Obj1 == null)
                {
                    list.Add(mod.Local);
                }
            }
        }
        _files.SetSelectItems(list);

        Source = _files.Source;
    }

    public void SetSelectWorld(WorldCloudModel item)
    {
        if (_selectWorld != null)
        {
            _selectWorld.IsSelect = false;
        }
        _selectWorld = item;
        _selectWorld.IsSelect = true;
    }

    public async void LoadWorld()
    {
        WorldCloudList.Clear();
        var res = await GameCloudUtils.GetWorldList();
        var worlds = await GameBinding.GetWorlds(Obj);
        if (res != null)
        {
            foreach (var item in res)
            {
                var obj = worlds.First(a => a.LevelName == item.Name);
                if (obj != null)
                {
                    worlds.Remove(obj);
                    WorldCloudList.Add(new(this, item, obj));
                }
                else
                {
                    WorldCloudList.Add(new(this, item));
                }
            }
        }
        foreach (var item in worlds)
        {
            WorldCloudList.Add(new(this, item));
        }
    }

    public void WindowClose()
    {
        OnPropertyChanged("WindowClose");
    }

    protected override void Close()
    {
        _files = null!;
        WorldCloudList.Clear();
        Source = null!;
    }

    public async void UploadWorld(WorldCloudModel world)
    {
        if (!world.HaveCloud)
        {
            Model.Progress("正在打包");
            string dir = world.World.Local;
            string local = Path.GetFullPath(dir + "/" + world.World.LevelName + ".zip");
            await new ZipUtils().ZipFile(local, dir);
            Model.ProgressUpdate("上传中");
            await GameCloudUtils.UploadWorld(Obj.UUID, local);
            PathHelper.Delete(local);
            Model.ProgressClose();
        }
        else
        { 
            
        }
    }

    public void DownloadWorld(WorldCloudModel world)
    {
        
    }
}
