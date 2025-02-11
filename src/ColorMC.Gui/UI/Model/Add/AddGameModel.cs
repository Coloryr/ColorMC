using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameModel : TopModel
{
    public const string NameTab1 = ":1";
    public const string NameTab2 = ":2";
    public const string NameTab3 = ":3";
    public const string NameTab4 = ":4";
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
    [ObservableProperty]
    private bool _main = true;

    public bool IsPhone { get; }

    public string? DefaultGroup { get; set; }

    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _load = false;

    private bool _keep = false;

    public AddGameModel(BaseModel model) : base(model)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        IsPhone = SystemInfo.Os == OsType.Android;

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
        var res = await Model.InputWithEditAsync(App.Lang("Text.Group"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(res.Text1))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error3"));
            return;
        }

        Model.Notify(App.Lang("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
        Group = res.Text1;
    }

    [RelayCommand]
    public void GoTab(object? arg)
    {
        Main = false;
        Model.PushBack(Back);
        OnPropertyChanged(arg as string);
    }

    [RelayCommand]
    public void GoModPack()
    {
        Main = false;
        Model.PushBack(BackDownload);
        OnPropertyChanged(NameTab1);
        if (!ConfigBinding.WindowMode())
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Info20"));
        }

        WindowManager.ShowAddModPack();
    }

    [RelayCommand]
    public void GoCloud()
    {
        Main = false;
        Model.PushBack(BackDownload);
        GameCloudDownload();
    }

    [RelayCommand]
    public void GoServer()
    {
        Main = false;
        Model.PushBack(BackDownload);
        ServerPackDownload();
    }

    [RelayCommand]
    public void GoDownload()
    {
        Model.PushBack(BackMain);
        Main = false;
        OnPropertyChanged(NameTab4);
    }

    public void BackMain()
    {
        Model.PopBack();
        OnPropertyChanged(NameBack);
        Main = true;
    }

    public void BackDownload()
    {
        Model.PopBack();
        OnPropertyChanged(NameTab4);
    }

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
    /// 请求
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        Model.ProgressClose();
        var test = await Model.ShowAsync(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
        Model.Progress();
        return test;
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private async Task<bool> GameRequest(string text)
    {
        Model.ProgressClose();
        var test = await Model.ShowAsync(text);
        Model.Progress();
        return test;
    }

    /// <summary>
    /// 添加进度
    /// </summary>
    /// <param name="state"></param>
    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            Model.ProgressUpdate(-1);
            if (!ConfigBinding.WindowMode())
            {
                Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info4"));
            }
            else
            {
                Model.ProgressClose();
            }
        }
        else if (state == CoreRunState.DownloadDone)
        {
            if (ConfigBinding.WindowMode())
            {
                Model.Progress(App.Lang("AddGameWindow.Tab2.Info4"));
            }
        }
        else if (state == CoreRunState.End)
        {
            Name = "";
            Group = "";
        }
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private async void Done(string? uuid)
    {
        Model.Notify(App.Lang("AddGameWindow.Tab1.Info7"));

        Name = "";

        if (_keep)
        {
            return;
        }

        var model = WindowManager.MainWindow?.DataContext as MainModel;
        model?.Select(uuid);

        var res = await Model.ShowAsync(App.Lang("AddGameWindow.Tab1.Info25"));
        if (res != true)
        {
            WindowClose();
        }
        else
        {
            _keep = true;
        }
    }
}
