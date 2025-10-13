using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// Macos下的左上角菜单
/// </summary>
public partial class AppModel : ObservableObject
{
    /// <summary>
    /// 显示主页面
    /// </summary>
    [RelayCommand]
    public void ShowMain()
    {
        WindowManager.ShowMain();
    }
    /// <summary>
    /// 显示设置
    /// </summary>
    [RelayCommand]
    public void ShowSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }
    /// <summary>
    /// 添加账户
    /// </summary>
    [RelayCommand]
    public void AddUser()
    {
        WindowManager.ShowUser(true);
    }
    /// <summary>
    /// 添加游戏实例
    /// </summary>
    [RelayCommand]
    public void AddGame()
    {
        WindowManager.ShowAddGame(null);
    }
    /// <summary>
    /// 退出启动器
    /// </summary>
    [RelayCommand]
    public void Exit()
    {
        ColorMCGui.Exit();
    }
}