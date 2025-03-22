using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameCloud;

/// <summary>
/// 游戏云同步窗口
/// </summary>
public partial class GameCloudModel : MenuModel
{
    /// <summary>
    /// 显示过滤
    /// </summary>
    [ObservableProperty]
    private bool _displayFilter = true;

    /// <summary>
    /// 导出的文件列表
    /// </summary>
    private FilesPage _files;

    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    /// <summary>
    /// 是否开启了云同步
    /// </summary>
    [ObservableProperty]
    private bool _enable;
    /// <summary>
    /// 上次配置文件同步时间
    /// </summary>
    [ObservableProperty]
    private string _configTime;
    /// <summary>
    /// 本地配置文件同步时间
    /// </summary>
    [ObservableProperty]
    private string _localConfigTime;

    /// <summary>
    /// 选择的存档
    /// </summary>
    private WorldCloudModel? _selectWorld;

    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Obj { get; init; }

    /// <summary>
    /// 云存档列表
    /// </summary>
    public ObservableCollection<WorldCloudModel> WorldCloudList { get; init; } = [];

    /// <summary>
    /// 游戏实例UUID
    /// </summary>
    public string UUID => Obj.UUID;

    private readonly string _useName;

    public GameCloudModel(BaseModel model, GameSettingObj obj) : base(model)
    {
        _useName = ToString() + ":" + obj.UUID;

        Obj = obj;

        SetHeadBack();

        LoadWorld();

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = App.Lang("GameCloudWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = App.Lang("GameCloudWindow.Tabs.Text2")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item4.svg",
                Text = App.Lang("GameCloudWindow.Tabs.Text3")
            }
        ]);
    }

    /// <summary>
    /// 开启云同步
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task MakeEnable()
    {
        if (Enable)
        {
            return;
        }

        Model.Progress(App.Lang("GameCloudWindow.Info3"));
        var res = await GameBinding.StartCloud(Obj);
        Model.ProgressClose();
        if (res.State == false)
        {
            Model.Show(res.Message!);
            return;
        }

        Model.Notify(App.Lang("GameCloudWindow.Info4"));
        Enable = true;
    }

    /// <summary>
    /// 关闭云同步
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task MakeDisable()
    {
        if (!Enable)
        {
            return;
        }

        var res = await Model.ShowAsync(App.Lang("GameCloudWindow.Info7"));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameCloudWindow.Info5"));
        var res1 = await GameBinding.StopCloud(Obj);
        Model.ProgressClose();
        if (!res1.State)
        {
            Model.Show(res1.Message!);
            return;
        }

        Model.Notify(App.Lang("GameCloudWindow.Info6"));
        Enable = false;
    }

    /// <summary>
    /// 上传配置文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task UploadConfig()
    {
        var files = _files.GetSelectItems(true);
        Model.Progress();
        var res = await GameBinding.UploadConfig(Obj, files, new()
        {
            Update = ProgressUpdate
        });
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
            return;
        }
        Model.Notify(App.Lang("GameCloudWindow.Info14"));
        await LoadCloud();
        LocalConfigTime = ConfigTime;
    }

    /// <summary>
    /// 下载配置文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadConfig()
    {
        Model.Progress();
        var res = await GameBinding.DownloadConfig(Obj, new()
        {
            Update = ProgressUpdate
        });
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
            return;
        }
        Model.Notify(App.Lang("GameCloudWindow.Info15"));
        await LoadCloud();
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
    }

    /// <summary>
    /// 获取游戏实例是否开启了云同步
    /// </summary>
    public async Task LoadCloud()
    {
        Model.Progress(App.Lang("GameCloudWindow.Info1"));
        var res = await GameBinding.HaveCloud(Obj);
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
            return;
        }
        Enable = res.Data1;
        ConfigTime = res.Data2 ?? App.Lang("GameCloudWindow.Info2");
    }

    /// <summary>
    /// 读取云同步信息
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Init()
    {
        if (!ColorMCCloudAPI.Connect)
        {
            Model.ShowWithOk(App.Lang("GameCloudWindow.Erro1"), WindowClose);
            return false;
        }
        await LoadCloud();
        await LoadConfig();

        return true;
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    public async Task LoadConfig()
    {
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
            foreach (var mod in await GameBinding.GetModFastAsync(Obj))
            {
                if (!Obj.Mods.Values.Any(a => a.Sha1 == mod.Sha1))
                {
                    list.Add(mod.Local);
                }
            }
        }
        _files.SetSelectItems(list);

        Source = _files.Source;
        Model.Notify(App.Lang("GameCloudWindow.Info19"));
    }

    /// <summary>
    /// 选择存档
    /// </summary>
    /// <param name="item">存档</param>
    public void SetSelectWorld(WorldCloudModel item)
    {
        if (_selectWorld != null)
        {
            _selectWorld.IsSelect = false;
        }
        _selectWorld = item;
        _selectWorld.IsSelect = true;
    }

    /// <summary>
    /// 加载存档列表
    /// </summary>
    public async void LoadWorld()
    {
        WorldCloudList.Clear();
        Model.Progress(App.Lang("GameCloudWindow.Info20"));
        var res = await GameBinding.GetCloudWorldListAsync(Obj);
        var worlds = await GameBinding.GetWorldsAsync(Obj);
        Model.ProgressClose();
        if (!res.State || res.Data == null)
        {
            Model.Show(res.Message!);
            return;
        }
        foreach (var item in res.Data)
        {
            var obj = worlds.Find(a => a.LevelName == item.Name);
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
        foreach (var item in worlds)
        {
            WorldCloudList.Add(new(this, item));
        }
        Model.Notify(App.Lang("GameCloudWindow.Info21"));
    }

    public override void Close()
    {
        _files = null!;
        foreach (var item in WorldCloudList)
        {
            item.Close();
        }
        WorldCloudList.Clear();
        RemoveHeadBack();
        Source = null!;
    }

    /// <summary>
    /// 上传存档
    /// </summary>
    /// <param name="world">云存档</param>
    public async void UploadWorld(WorldCloudModel world)
    {
        Model.Progress();
        var res = await GameBinding.UploadCloudWorldAsync(Obj, world, new()
        {
            Update = ProgressUpdate
        });
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
            return;
        }
        Model.Notify(App.Lang("GameCloudWindow.Info14"));
        LoadWorld();
    }

    /// <summary>
    /// 下载存档
    /// </summary>
    /// <param name="world">云存档</param>
    public async void DownloadWorld(WorldCloudModel world)
    {
        Model.Progress();
        var res = await GameBinding.DownloadCloudWorldAsync(Obj, world, new()
        {
            Update = ProgressUpdate
        });
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
            return;
        }
        Model.Notify(App.Lang("GameCloudWindow.Info15"));
        LoadWorld();
    }

    /// <summary>
    /// 删除云存档
    /// </summary>
    /// <param name="world">云存档</param>
    public async void DeleteCloud(WorldCloudModel world)
    {
        var res = await Model.ShowAsync(App.Lang("GameCloudWindow.Info16"));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameCloudWindow.Info18"));
        var res1 = await GameBinding.DeleteCloudWorld(Obj, world.Cloud.Name);
        Model.ProgressClose();
        if (!res1.State)
        {
            Model.Show(res1.Message!);
        }
        else
        {
            Model.Notify(App.Lang("GameCloudWindow.Info17"));
        }
    }

    /// <summary>
    /// 重载内容
    /// </summary>
    private async void Reload()
    {
        switch (NowView)
        {
            case 0:
                await LoadCloud();
                break;
            case 1:
                await LoadConfig();
                break;
            case 2:
                LoadWorld();
                break;
        }
    }

    /// <summary>
    /// 下载进度更新
    /// </summary>
    /// <param name="data"></param>
    private void ProgressUpdate(string data)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Model.ProgressUpdate(data);
        });
    }

    /// <summary>
    /// 设置标题栏
    /// </summary>
    public void SetHeadBack()
    {
        Model.SetChoiseContent(_useName, App.Lang("Button.Refash"));
        Model.SetChoiseCall(_useName, Reload);
    }

    /// <summary>
    /// 清理标题栏
    /// </summary>
    public void RemoveHeadBack()
    {
        Model.RemoveChoiseData(_useName);
    }
}
