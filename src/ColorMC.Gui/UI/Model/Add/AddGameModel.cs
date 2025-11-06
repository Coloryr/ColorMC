using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏实例
/// </summary>
public partial class AddGameModel : TopModel
{
    public const string NameTab1 = ":1";
    public const string NameTab2 = ":2";
    public const string NameTab3 = ":3";
    public const string NameBack = "Back";

    /// <summary>
    /// 游戏分组
    /// </summary>
    public ObservableCollection<string> GroupList { get; init; } = [];

    /// <summary>
    /// 实例名字
    /// </summary>
    [ObservableProperty]
    private string? _name;
    /// <summary>
    /// 实例组
    /// </summary>
    [ObservableProperty]
    private string? _group;

    /// <summary>
    /// 云同步启用
    /// </summary>
    [ObservableProperty]
    private bool _cloudEnable;
    /// <summary>
    /// 是否为主界面
    /// </summary>
    [ObservableProperty]
    private bool _main = true;

    /// <summary>
    /// 添加到的分组
    /// </summary>
    public string? DefaultGroup { get; set; }

    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _load = false;
    /// <summary>
    /// 是否继续添加
    /// </summary>
    private bool _keep = false;

    public AddGameModel(BaseModel model) : base(model)
    {
        GroupList.Clear();
        GroupList.AddRange(InstancesPath.GroupKeys);

        GameVersionUpdate();

        CloudEnable = ColorMCCloudAPI.Connect;
    }

    /// <summary>
    /// 添加新的游戏分组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGroup()
    {
        var res = await Model.InputWithEditAsync(LanguageUtils.Get("Text.Group"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(res.Text1))
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Error3"));
            return;
        }

        Model.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(InstancesPath.GroupKeys);
        Group = res.Text1;
    }

    /// <summary>
    /// 转到菜单
    /// </summary>
    /// <param name="arg">目标菜单</param>
    [RelayCommand]
    public void GoTab(object? arg)
    {
        Main = false;
        Model.PushBack(Back);
        OnPropertyChanged(arg as string);
    }

    /// <summary>
    /// 转到添加整合包
    /// </summary>
    [RelayCommand]
    public void GoModPack()
    {
        Main = false;
        Model.PushBack(BackMain);
        OnPropertyChanged(NameTab1);
        if (!ConfigBinding.WindowMode())
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Info20"));
        }

        WindowManager.ShowAddModPack();
    }

    /// <summary>
    /// 转到添加云同步实例
    /// </summary>
    [RelayCommand]
    public void GoCloud()
    {
        Main = false;
        Model.PushBack(BackMain);
        GameCloudDownload();
    }

    /// <summary>
    /// 转到添加服务器实例
    /// </summary>
    [RelayCommand]
    public void GoServer()
    {
        Main = false;
        Model.PushBack(BackMain);
        ServerPackDownload();
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void BackMain()
    {
        Model.PopBack();
        OnPropertyChanged(NameBack);
        Main = true;
    }

    /// <summary>
    /// 返回
    /// </summary>
    private void Back()
    {
        Model.PopBack();
        Name = null;
        Group = DefaultGroup;
        Version = null;
        LoaderVersion = null;
        LoaderTypeList.Clear();
        LoaderType = -1;
        LoaderLocal = null;
        _loaderTypeList.Clear();
        LoaderVersionList.Clear();
        Type = null;
        ZipLocal = null;
        _fileModel = null;
        SelectPath = null;
        Files = null;
        OnPropertyChanged("Back");
        Main = true;
    }

    public override void Close()
    {
        _load = true;
        Back();
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        LoaderTypeList.Clear();
        _fileModel = null!;
        Files = null!;
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private async void Done(string? uuid)
    {
        Name = "";
        Group = "";

        Model.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Info7"));

        if (_keep)
        {
            return;
        }

        GameBinding.SelectAndReloadGame(uuid);

        var res = await Model.ShowAsync(LanguageUtils.Get("AddGameWindow.Tab1.Info25"));
        if (res != true)
        {
            WindowClose();
        }
        else
        {
            _keep = true;
        }
    }

    /// <summary>
    /// 下载云同步游戏实例
    /// </summary>
    /// <returns></returns>
    public async void GameCloudDownload()
    {
        Model.Progress(LanguageUtils.Get("AddGameWindow.Tab1.Info9"));
        var list = await ColorMCCloudAPI.GetListAsync();
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Error9"));
            return;
        }
        var list1 = new List<string>();
        list.ForEach(item =>
        {
            if (!string.IsNullOrWhiteSpace(item.Name) && InstancesPath.GetGame(item.UUID) == null)
            {
                list1.Add(item.Name);
            }
        });
        var res = await Model.ShowCombo(LanguageUtils.Get("AddGameWindow.Tab1.Info10"), list1);
        if (res.Cancel)
        {
            return;
        }

        Model.Progress(LanguageUtils.Get("AddGameWindow.Tab1.Info11"));
        var obj = list[res.Index];
        while (true)
        {
            //替换冲突的名字
            if (InstancesPath.GetGameByName(obj.Name) != null)
            {
                var res1 = await Model.ShowAsync(LanguageUtils.Get("AddGameWindow.Tab1.Info12"));
                if (!res1)
                {
                    Model.ProgressClose();
                    return;
                }
                var res2 = await Model.Input(LanguageUtils.Get("AddGameWindow.Tab1.Text2"), obj.Name);
                if (res2.Cancel)
                {
                    return;
                }

                obj.Name = res2.Text1!;
            }
            else
            {
                break;
            }
        }
        //下载游戏实例
        var res3 = await GameBinding.DownloadCloudAsync(obj, Group,
            new CreateGameGui(Model));
        Model.ProgressClose();
        if (!res3.State)
        {
            Model.Show(res3.Data!);
            return;
        }

        WindowManager.ShowGameCloud(InstancesPath.GetGame(obj.UUID!)!);
        Done(res3.Data);
    }

    /// <summary>
    /// 下载服务器实例
    /// </summary>
    public async void ServerPackDownload()
    {
        var res = await Model.InputWithEditAsync(LanguageUtils.Get("AddGameWindow.Tab1.Info13"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Error14"));
            return;
        }

        if (!res.Text1.EndsWith('/'))
        {
            res.Text1 += '/';
        }
        //下载服务器包
        Model.Progress(LanguageUtils.Get("AddGameWindow.Tab1.Info14"));
        var res1 = await GameBinding.DownloadServerPackAsync(Model, Name, Group, res.Text1,
            new CreateGameGui(Model));
        Model.ProgressClose();
        if (!res1.State)
        {
            if (res1.Data != null)
            {
                Model.Show(res1.Data!);
            }
        }
        else
        {
            Done(res1.Data!);
        }
    }
}
