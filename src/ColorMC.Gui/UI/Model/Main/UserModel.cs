using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel
{
    /// <summary>
    /// 用户名字
    /// </summary>
    [ObservableProperty]
    private string _userId;
    /// <summary>
    /// 用户类型
    /// </summary>
    [ObservableProperty]
    private string _userType;

    /// <summary>
    /// 头像
    /// </summary>
    [ObservableProperty]
    private Bitmap _head = ImageManager.LoadBitmap;

    /// <summary>
    /// 是否有头像
    /// </summary>
    [ObservableProperty]
    private bool _isHeadLoad;
    /// <summary>
    /// 是否为正版账户
    /// </summary>
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
            if (GuiConfigUtils.Config.Card.Online)
            {
                IsOnlineMode = user.AuthType == AuthType.OAuth;
            }
            else
            {
                IsOnlineMode = false;
            }

            UserId = user.UserName;
            //锁定用户切换
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

    /// <summary>
    /// 重载皮肤
    /// </summary>
    private void SkinChange()
    {
        Head = ImageManager.HeadBitmap!;

        IsHeadLoad = false;
    }
}
