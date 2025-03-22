using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// Macos�µ����Ͻǲ˵�
/// </summary>
public partial class AppModel : ObservableObject
{
    /// <summary>
    /// ��ʾ��ҳ��
    /// </summary>
    [RelayCommand]
    public void ShowMain()
    {
        WindowManager.ShowMain();
    }
    /// <summary>
    /// ��ʾ����
    /// </summary>
    [RelayCommand]
    public void ShowSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }
    /// <summary>
    /// ����˻�
    /// </summary>
    [RelayCommand]
    public void AddUser()
    {
        WindowManager.ShowUser(true);
    }
    /// <summary>
    /// �����Ϸʵ��
    /// </summary>
    [RelayCommand]
    public void AddGame()
    {
        WindowManager.ShowAddGame(null);
    }
    /// <summary>
    /// �˳�������
    /// </summary>
    [RelayCommand]
    public void Exit()
    {
        ColorMCGui.Exit();
    }
}