using System.Collections.Generic;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Login;

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
