using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model;

public partial class AppModel : ObservableObject
{
    [RelayCommand]
    public void ShowMain()
    {
        WindowManager.ShowMain();
    }

    [RelayCommand]
    public void ShowSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }

    [RelayCommand]
    public void AddUser()
    {
        WindowManager.ShowUser(true);
    }

    [RelayCommand]
    public void AddGame()
    {
        WindowManager.ShowAddGame(null);
    }

    [RelayCommand]
    public void Exit()
    {
        App.Exit();
    }
}