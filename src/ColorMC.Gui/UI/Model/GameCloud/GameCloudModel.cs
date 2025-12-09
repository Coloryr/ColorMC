using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Dialog;
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
    public string UUID => Obj.UUID.ToString();

    private readonly string _useName;

    public GameCloudModel(WindowModel model, GameSettingObj obj) : base(model)
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
                Text = LanguageUtils.Get("GameCloudWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = LanguageUtils.Get("Text.Config")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item4.svg",
                Text = LanguageUtils.Get("GameCloudWindow.Tabs.Text3")
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

        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text3"));
        var res = await GameBinding.StartCloudAsync(Obj);
        Window.CloseDialog(dialog);
        if (res.State == false)
        {
            Window.Show(res.Data!);
            return;
        }

        Window.Notify(LanguageUtils.Get("GameCloudWindow.Text4"));
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

        var res = await Window.ShowChoice(LanguageUtils.Get("GameCloudWindow.Text7"));
        if (!res)
        {
            return;
        }

        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text5"));
        var res1 = await GameBinding.StopCloudAsync(Obj);
        Window.CloseDialog(dialog);
        if (!res1.State)
        {
            Window.Show(res1.Data!);
            return;
        }

        Window.Notify(LanguageUtils.Get("GameCloudWindow.Text6"));
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
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text9"));
        var res = await GameBinding.UploadConfigAsync(Obj, files, dialog);
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(res.Data!);
            return;
        }
        Window.Notify(LanguageUtils.Get("GameCloudWindow.Text14"));
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
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text32"));
        var res = await GameBinding.DownloadConfigAsync(Obj, dialog);
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(res.Data!);
            return;
        }
        Window.Notify(LanguageUtils.Get("GameCloudWindow.Text15"));
        await LoadCloud();
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
    }

    /// <summary>
    /// 获取游戏实例是否开启了云同步
    /// </summary>
    public async Task LoadCloud()
    {
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text1"));
        var res = await GameBinding.HaveCloudAsync(Obj);
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(res.Data!);
            return;
        }
        Enable = res.Data1;
        ConfigTime = res.Data2 ?? LanguageUtils.Get("GameCloudWindow.Text2");
    }

    /// <summary>
    /// 读取云同步信息
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Init()
    {
        if (!ColorMCCloudAPI.Connect)
        {
            await Window.ShowDialogWait(new ChoiceModel(Window.WindowId)
            {
                Text = LanguageUtils.Get("GameCloudWindow.Erro1")
            });
            WindowClose();
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
            Path.Combine(Obj.GetBasePath(), Names.NameModPackFile),
            Path.Combine(Obj.GetBasePath(), Names.NameModrinthFile)
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
        Window.Notify(LanguageUtils.Get("GameCloudWindow.Text19"));
    }

    /// <summary>
    /// 选择存档
    /// </summary>
    /// <param name="item">存档</param>
    public void SetSelectWorld(WorldCloudModel item)
    {
        _selectWorld?.IsSelect = false;
        _selectWorld = item;
        _selectWorld.IsSelect = true;
    }

    /// <summary>
    /// 加载存档列表
    /// </summary>
    public async void LoadWorld()
    {
        WorldCloudList.Clear();
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameEditWindow.Tab5.Text10"));
        var res = await GameBinding.GetCloudWorldListAsync(Obj);
        var worlds = await Obj.GetSavesAsync();
        Window.CloseDialog(dialog);
        if (!res.State || res.Worlds == null)
        {
            Window.Show(res.Data!);
            return;
        }
        foreach (var item in res.Worlds)
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
        Window.Notify(LanguageUtils.Get("GameEditWindow.Tab5.Text21"));
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
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text34"));
        var res = await GameBinding.UploadCloudWorldAsync(Obj, world, dialog);
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(res.Data!);
            return;
        }
        Window.Notify(LanguageUtils.Get("GameCloudWindow.Text14"));
        LoadWorld();
    }

    /// <summary>
    /// 下载存档
    /// </summary>
    /// <param name="world">云存档</param>
    public async void DownloadWorld(WorldCloudModel world)
    {
        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text36"));
        var res = await GameBinding.DownloadCloudWorldAsync(Obj, world, dialog);
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(res.Data!);
            return;
        }
        Window.Notify(LanguageUtils.Get("GameCloudWindow.Text15"));
        LoadWorld();
    }

    /// <summary>
    /// 删除云存档
    /// </summary>
    /// <param name="world">云存档</param>
    public async void DeleteCloud(WorldCloudModel world)
    {
        var res = await Window.ShowChoice(LanguageUtils.Get("GameCloudWindow.Text16"));
        if (!res)
        {
            return;
        }

        var dialog = Window.ShowProgress(LanguageUtils.Get("GameCloudWindow.Text18"));
        var res1 = await GameBinding.DeleteCloudWorldAsync(Obj, world.Cloud.Name);
        Window.CloseDialog(dialog);
        if (!res1.State)
        {
            Window.Show(res1.Data!);
        }
        else
        {
            Window.Notify(LanguageUtils.Get("Text.DeleteDone"));
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
    /// 设置标题栏
    /// </summary>
    public void SetHeadBack()
    {
        Window.SetChoiseContent(_useName, LanguageUtils.Get("Button.Refash"));
        Window.SetChoiseCall(_useName, Reload);
    }

    /// <summary>
    /// 清理标题栏
    /// </summary>
    public void RemoveHeadBack()
    {
        Window.RemoveChoiseData(_useName);
    }
}
