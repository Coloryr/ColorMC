using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    [ObservableProperty]
    private string _userId;
    [ObservableProperty]
    private string _userType;

    [ObservableProperty]
    private Bitmap _head = ImageManager.LoadIcon;

    [ObservableProperty]
    private bool _isHeadLoad;
    [ObservableProperty]
    private bool _isOnlineMode;

    /// <summary>
    /// 加载用户信息
    /// </summary>
    public async void LoadUser()
    {
        IsHeadLoad = true;

        var user = UserBinding.GetLastUser();

        if (user == null)
        {
            UserId = App.Lang("MainWindow.Info36");
            UserType = App.Lang("MainWindow.Info35");
        }
        else
        {
            IsOnlineMode = user.AuthType == AuthType.OAuth;

            UserId = user.UserName;

            if (GuiConfigUtils.Config.ServerCustom.LockLogin && user.AuthType != AuthType.OAuth)
            {
                bool find = false;
                foreach (var item in GuiConfigUtils.Config.ServerCustom.LockLogins)
                {
                    if (item.Type == user.AuthType && item.Data == user.Text1)
                    {
                        UserType = item.Name;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    UserType = App.Lang("MainWindow.Error7");
                }
            }
            else
            {
                UserType = user.AuthType.GetName();
            }
        }

        await UserBinding.LoadSkin();
    }
}
