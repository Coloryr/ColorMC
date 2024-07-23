using System.Collections.ObjectModel;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.CustomGui;

public partial class UIModel(BaseModel model) : TopModel(model)
{
    /// <summary>
    /// 游戏列表
    /// </summary>
    public ObservableCollection<GameItemModel> Games { get; } = [];

    /// <summary>
    /// 用于展示的服务器
    /// </summary>
    public (string, ushort) IPPort => ("www.coloryr.com", 25565);

    /// <summary>
    /// 选中的游戏实例
    /// </summary>
    [ObservableProperty]
    public GameItemModel? _selectGame;

    /// <summary>
    /// 打开启动器设置指令
    /// </summary>
    [RelayCommand]
    public void OpenSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }

    /// <summary>
    /// 打开用户列表指令
    /// </summary>
    [RelayCommand]
    public void OpenUsers()
    {
        WindowManager.ShowUser();
    }

    /// <summary>
    /// 启动游戏指令
    /// </summary>
    [RelayCommand]
    public async Task Launch()
    {
        if (SelectGame == null)
        {
            Model.Show("你还没有选择游戏");
            return;
        }

        Model.Progress("正在启动游戏");
        var res = await GameBinding.Launch(Model, SelectGame.Obj, hide: true);
        Model.ProgressClose();
        if (!res.Item1)
        {
            Model.Show("游戏启动失败\n" + res.Item2);
        }
    }

    public override void Close()
    {

    }

    /// <summary>
    /// 加载游戏列表
    /// </summary>
    public void Load()
    {
        Games.Clear();
        foreach (var item in InstancesPath.Games)
        {
            Games.Add(new(Model, null, item));
        }
    }
}
