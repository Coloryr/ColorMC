using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

public static class Language
{
    public static string GetName(this AuthType type)
    {
        return type switch
        {
            AuthType.Offline => "离线",
            AuthType.OAuth => "微软登录",
            AuthType.Nide8 => "统一通行证",
            AuthType.AuthlibInjector => "外置登录",
            AuthType.LittleSkin => "皮肤站",
            AuthType.SelfLittleSkin => "自建皮肤站",
            _ =>"账户"
        };
    }

    public static string GetName(this AuthState state)
    {
        return state switch
        {
            AuthState.OAuth => "OAuth",
            AuthState.XBox => "XBoxLive",
            AuthState.XSTS => "XSTS",
            AuthState.Token => "Token",
            AuthState.Profile => "Profile",
            _ => "登录状态未知"
        };
    }

    public static string GetName(this LoginState state)
    {
        return state switch
        {
            LoginState.Done => "完成",
            LoginState.TimeOut => "超时",
            LoginState.JsonError => "数据错误",
            LoginState.Error => "未知错误",
            LoginState.ErrorType => "错误的账户类型",
            LoginState.Crash => "崩溃",
            _ => "登录结果未知"
        };
    }

    public static string GetName(this LaunchState state)
    {
        return state switch
        {
            LaunchState.Check => "验证游戏完整性",
            LaunchState.CheckVersion => "验证游戏核心",
            LaunchState.CheckLib => "验证游戏运行库",
            LaunchState.CheckAssets => "验证游戏资源文件",
            LaunchState.CheckLoader => "验证游戏Mod加载器",
            LaunchState.CheckLoginCore => "验证外置登录核心",
            LaunchState.LostVersion => "缺少游戏核心",
            LaunchState.LostLib => "缺少运行库",
            LaunchState.LostLoader => "缺少Mod加载器",
            LaunchState.LostLoginCore => "缺少外置登录核心",
            LaunchState.Download => "下载文件",
            LaunchState.JvmPrepare => "准备Jvm参数",
            LaunchState.VersionError => "游戏核心错误",
            LaunchState.AssetsError => "游戏资源文件错误",
            LaunchState.LoaderError => "Mod加载器错误",
            LaunchState.JvmError => "没有合适的Java",
            _ => "启动状态未知"
        };
    }
}
