using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;

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
            _ => "账户"
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

    public static string GetName(this DownloadState state)
    {
        return state switch
        {
            DownloadState.Init => "初始化",
            DownloadState.GetInfo => "获取信息",
            DownloadState.End => "结束",
            _ => "未知的状态"
        };
    }

    public static string GetName(this DownloadItemState state)
    {
        return state switch
        {
            DownloadItemState.Wait => "等待中",
            DownloadItemState.Download => "下载中",
            DownloadItemState.Init => "初始化中",
            DownloadItemState.Action => "执行后续操作中",
            DownloadItemState.Done => "完成",
            DownloadItemState.Error => "发生错误",
            _ => "未知的状态"
        };
    }

    public static string GetName(this SortField state)
    {
        return state switch
        {
            SortField.Featured => "特色",
            SortField.Popularity => "热度",
            SortField.LastUpdated => "最后更新",
            SortField.Name => "名字",
            SortField.Author => "作者",
            SortField.TotalDownloads => "下载数",
            SortField.Category => "类别",
            SortField.GameVersion => "游戏版本",
            _ => "未知的分类"
        };
    }

    public static string GetNameWithRelease(int type)
    {
        return type switch
        {
            1 => "发布",
            2 => "Beta测试版",
            3 => "Alpha测试版",
            _ => "未知的发布类型"
        };
    }

    public static string GetName(this SourceLocal state)
    {
        return state switch
        {
            SourceLocal.Offical => "官方",
            SourceLocal.BMCLAPI => "BmclApi",
            SourceLocal.MCBBS => "Mcbbs",
            _ =>"未知的下载源"
        };
    }

    public static string GetName(this GCType state)
    {
        return state switch
        {
            GCType.G1GC => "G1垃圾回收器",
            GCType.SerialGC => "串行垃圾回收器",
            GCType.ParallelGC => "并行垃圾回收器",
            GCType.CMSGC => "并发标记扫描垃圾回收器",
            GCType.User => "用户设置",
            _ => "未知的GC类型"
        };
    }
}
