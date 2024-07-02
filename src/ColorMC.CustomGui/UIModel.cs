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

public partial class UIModel(BaseModel model) : TopModel(model), IMainTop
{
    public ObservableCollection<GameItemModel> Games { get; } = [];

    public (string, ushort) IPPort => ("www.coloryr.com", 25565);

    [ObservableProperty]
    public GameItemModel? _selectGame;

    [RelayCommand]
    public void OpenSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }

    [RelayCommand]
    public void OpenUsers()
    {
        WindowManager.ShowUser();
    }

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

    protected override void Close()
    {
        
    }

    public void Load()
    {
        Games.Clear();
        foreach (var item in InstancesPath.Games)
        {
            Games.Add(new(Model, this, item));
        }
    }

    public void Launch(GameItemModel obj)
    {
        
    }

    public void Select(GameItemModel? model)
    {
        
    }

    public void EditGroup(GameItemModel model)
    {
        
    }
}
