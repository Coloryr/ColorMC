using ColorMC.Core.Objs;

namespace ColorMC.Core.Helpers;

public static class ConfigHelper
{
    public static RunArgObj Copy(this RunArgObj arg)
    {
        return new()
        {
            JvmArgs = arg.JvmArgs,
            GameArgs = arg.GameArgs,
            GCArgument = arg.GCArgument,
            JvmEnv = arg.JvmEnv,
            GC = arg.GC,
            JavaAgent = arg.JavaAgent,
            MaxMemory = arg.MaxMemory,
            MinMemory = arg.MinMemory,
            LaunchPre = arg.LaunchPre,
            LaunchPreData = arg.LaunchPreData,
            LaunchPost = arg.LaunchPost,
            LaunchPostData = arg.LaunchPostData,
        };
    }

    public static WindowSettingObj Copy(this WindowSettingObj arg)
    {
        return new()
        {
            FullScreen = arg.FullScreen,
            Height = arg.Height,
            Width = arg.Width,
        };
    }

    public static ServerObj Copy(this ServerObj arg)
    {
        return new()
        {
            IP = arg.IP,
            Port = arg.Port
        };
    }

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

    public static AdvanceJvmObj Copy(this AdvanceJvmObj arg)
    {
        return new()
        {
            MainClass = arg.MainClass,
            ClassPath = arg.ClassPath
        };
    }
}
