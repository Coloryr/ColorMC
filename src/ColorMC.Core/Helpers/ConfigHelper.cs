using ColorMC.Core.Objs;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 配置文件处理
/// </summary>
public static class ConfigHelper
{
    /// <summary>
    /// 复制配置文件
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static RunArgObj Copy(this RunArgObj arg)
    {
        return new()
        {
            JvmArgs = arg.JvmArgs,
            GameArgs = arg.GameArgs,
            JvmEnv = arg.JvmEnv,
            GC = arg.GC,
            MaxMemory = arg.MaxMemory,
            MinMemory = arg.MinMemory,
            LaunchPre = arg.LaunchPre,
            LaunchPreData = arg.LaunchPreData,
            LaunchPost = arg.LaunchPost,
            LaunchPostData = arg.LaunchPostData,
        };
    }

    /// <summary>
    /// 复制配置文件
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static WindowSettingObj Copy(this WindowSettingObj arg)
    {
        return new()
        {
            FullScreen = arg.FullScreen,
            Height = arg.Height,
            Width = arg.Width,
        };
    }

    /// <summary>
    /// 复制配置文件
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static ServerObj Copy(this ServerObj arg)
    {
        return new()
        {
            IP = arg.IP,
            Port = arg.Port
        };
    }

    /// <summary>
    /// 复制配置文件
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static ProxyHostObj Copy(this ProxyHostObj arg)
    {
        return new()
        {
            IP = arg.IP,
            Port = arg.Port,
            User = arg.User,
            Password = arg.Password
        };
    }

    /// <summary>
    /// 复制配置文件
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static AdvanceJvmObj Copy(this AdvanceJvmObj arg)
    {
        return new()
        {
            MainClass = arg.MainClass,
            ClassPath = arg.ClassPath
        };
    }
}
