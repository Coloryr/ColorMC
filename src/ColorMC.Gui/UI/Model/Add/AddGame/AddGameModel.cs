using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add.AddGame;

public partial class AddGameModel : MenuModel
{
    /// <summary>
    /// 游戏分组
    /// </summary>
    public ObservableCollection<string> GroupList { get; init; } = new();

    public override List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/AddMenu/item1.svg",
            Text = App.Lang("AddGameWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/AddMenu/item2.svg",
            Text = App.Lang("AddGameWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/AddMenu/item3.svg",
            Text = App.Lang("AddGameWindow.Tabs.Text3") },
    };

    /// <summary>
    /// 实例名字
    /// </summary>
    [ObservableProperty]
    private string _name;
    /// <summary>
    /// 实例组
    /// </summary>
    [ObservableProperty]
    private string _group;

    /// <summary>
    /// 云同步启用
    /// </summary>
    [ObservableProperty]
    private bool _cloudEnable;

    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _load = false;

    public AddGameModel(BaseModel model) : base(model)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        GameVersionUpdate();

        ColorMCCore.PackState = PackState;
        ColorMCCore.PackUpdate = PackUpdate;

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

    protected override void Close()
    {
        ColorMCCore.GameOverwirte = null;
        ColorMCCore.GameRequest = null;
        _load = true;
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        _fileModel = null!;
        Files = null!;
    }
}
