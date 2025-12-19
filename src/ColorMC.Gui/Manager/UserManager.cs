using System.Collections.Generic;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 账户管理
/// </summary>
public static class UserManager
{
    /// <summary>
    /// 锁定的账户类型
    /// </summary>
    private static readonly List<UserKeyObj> s_lockUser = [];

    private static LoginObj? s_select;

    public static void Init()
    {
        if (GuiConfigUtils.Config.LastUser is { } last)
        {
            s_select = AuthDatabase.Get(last.UUID, last.Type);
        }
    }

    /// <summary>
    /// 编辑账户
    /// </summary>
    /// <param name="obj">原来的账户</param>
    /// <param name="text1">新的名字</param>
    /// <param name="text2">新的UUID</param>
    public static void EditUser(LoginObj obj, string text1, string text2)
    {
        foreach (var item in AuthDatabase.Auths.Values)
        {
            if (item.UserName == obj.UserName && item.UUID == obj.UUID && item.AuthType == AuthType.Offline)
            {
                item.UserName = text1;
                item.UUID = text2;
                AuthDatabase.Save();
                break;
            }
        }
    }

    /// <summary>
    /// 清空所有账户
    /// </summary>
    public static void ClearAllUser()
    {
        AuthDatabase.ClearAuths();

        GuiConfigUtils.Config.LastUser = null;
        GuiConfigUtils.Save();

        EventManager.OnLastUserChange(false);
    }

    /// <summary>
    /// 获取所有账户
    /// </summary>
    /// <returns></returns>
    public static LoginObj? GetLastUser()
    {
        return s_select;
    }

    /// <summary>
    /// 设置选中账户
    /// </summary>
    /// <param name="login"></param>
    public static void SetSelect(LoginObj? login)
    {
        s_select = login;

        if (login != null)
        {
            GuiConfigUtils.Config.LastUser = new LastUserSetting
            {
                Type = login.AuthType,
                UUID = login.UUID
            };
            GuiConfigUtils.Save();

            EventManager.OnLastUserChange(true);
        }
        else
        {
            GuiConfigUtils.Config.LastUser = null;
            GuiConfigUtils.Save();

            EventManager.OnLastUserChange(false);
        }
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <param name="type">账户类型</param>
    public static void Remove(string uuid, AuthType type)
    {
        if (s_select != null && type == s_select.AuthType && uuid == s_select.UUID)
        {
            SetSelect(null);
            GuiConfigUtils.Config.LastUser = null;
            GuiConfigUtils.Save();
        }
        AuthDatabase.Get(uuid, type)?.Delete();
    }

    /// <summary>
    /// 锁定账户
    /// </summary>
    /// <param name="obj">账户</param>
    public static void LockUser(LoginObj obj)
    {
        var key = obj.GetKey();
        if (!s_lockUser.Contains(key))
        {
            s_lockUser.Add(key);
        }
    }

    /// <summary>
    /// 解锁账户
    /// </summary>
    /// <param name="obj">账户</param>
    public static void UnlockUser(LoginObj obj)
    {
        s_lockUser.Remove(obj.GetKey());
    }

    /// <summary>
    /// 账户是否锁定
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    public static bool IsLock(LoginObj obj)
    {
        return s_lockUser.Contains(obj.GetKey());
    }
}
