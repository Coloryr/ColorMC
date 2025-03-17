using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 账户相关处理
/// </summary>
public static class UserHelper
{
    /// <summary>
    /// 获取用户键
    /// </summary>
    /// <param name="login">账户</param>
    /// <returns>用户键</returns>
    public static UserKeyObj GetKey(this LoginObj login)
    {
        return new() { UUID = login.UUID, Type = login.AuthType };
    }
}
