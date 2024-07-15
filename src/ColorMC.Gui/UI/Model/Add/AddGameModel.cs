using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameModel : TopModel
{
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

    public AddGameModel(BaseModel model) : base(model)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        IsPhone = SystemInfo.Os == OsType.Android;

        GameVersionUpdate();

        CloudEnable = GameCloudUtils.Connect;
    }

    /// <summary>
    /// 添加新的游戏分组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("AddGameWindow.Tab1.Info5"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error3"));
            return;
        }

        Model.Notify(App.Lang("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
        Group = Text;
    }

    [RelayCommand]
    public void GoTab(object? arg)
    {
        Main = false;
        Model.PushBack(Back);
        OnPropertyChanged("Go" + arg);
    }

    [RelayCommand]
    public void GoModPack()
    {
        Main = false;
        Model.PushBack(BackDownload);
        WindowManager.ShowAddModPack();
        OnPropertyChanged("GoTab1");
        Model.Show(App.Lang("AddGameWindow.Tab1.Info20"));
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
        OnPropertyChanged("GoDownload");
    }

    public void BackMain()
    {
        Model.PopBack();
        OnPropertyChanged("Back");
        Main = true;
    }

    public void BackDownload()
    {
        Model.PopBack();
        OnPropertyChanged("GoDownload");
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
}
