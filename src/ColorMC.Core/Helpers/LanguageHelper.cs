using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using System.Reflection;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 语言文件
/// </summary>
public static class LanguageHelper
{
    private static readonly Language Language = new();
    private static LanguageType NowType;

    /// <summary>
    /// 加载语言文件
    /// </summary>
    public static void Load(LanguageType type)
    {
        string name = type switch
        {
            //LanguageType.en_us => "ColorMC.Core.Resources.Language.core_en-us.json",
            _ => "ColorMC.Core.Resources.Language.core_zh-cn.json"
        };
        Assembly assm = Assembly.GetExecutingAssembly();
        using var istr = assm.GetManifestResourceStream(name)!;
        Language.Load(istr);
    }

    /// <summary>
    /// 切换语言文件
    /// </summary>
    public static void Change(LanguageType type)
    {
        if (NowType == type)
            return;

        NowType = type;
        Load(type);
        ColorMCCore.LanguageReload?.Invoke(type);
    }

    /// <summary>
    /// 取语言
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Get(string input)
    {
        return Language.GetLanguage(input);
    }

    public static string GetName(this AuthType type)
    {
        return type switch
        {
            AuthType.Offline => Get("AuthType.Offline"),
            AuthType.OAuth => Get("AuthType.OAuth"),
            AuthType.Nide8 => Get("AuthType.Nide8"),
            AuthType.AuthlibInjector => Get("AuthType.AuthlibInjector"),
            AuthType.LittleSkin => Get("AuthType.LittleSkin"),
            AuthType.SelfLittleSkin => Get("AuthType.SelfLittleSkin"),
            _ => Get("AuthType.Other")
        };
    }

    public static string GetName(this AuthState state)
    {
        return state switch
        {
            AuthState.OAuth => Get("AuthType.Other"),
            AuthState.XBox => Get("AuthType.XBox"),
            AuthState.XSTS => Get("AuthType.XSTS"),
            AuthState.Token => Get("AuthType.Token"),
            AuthState.Profile => Get("AuthType.Profile"),
            _ => Get("AuthType.Other")
        };
    }

    public static string GetName(this LoginState state)
    {
        return state switch
        {
            LoginState.Done => Get("LoginState.Done"),
            LoginState.TimeOut => Get("LoginState.TimeOut"),
            LoginState.JsonError => Get("LoginState.JsonError"),
            LoginState.Error => Get("LoginState.Error"),
            LoginState.Crash => Get("LoginState.Crash"),
            _ => Get("LoginState.Other")
        };
    }

    public static string GetName(this LaunchState state)
    {
        return state switch
        {
            LaunchState.Check => Get("LaunchState.Check"),
            LaunchState.CheckVersion => Get("LaunchState.CheckVersion"),
            LaunchState.CheckLib => Get("LaunchState.CheckLib"),
            LaunchState.CheckAssets => Get("LaunchState.CheckAssets"),
            LaunchState.CheckLoader => Get("LaunchState.CheckLoader"),
            LaunchState.CheckLoginCore => Get("LaunchState.CheckLoginCore"),
            LaunchState.LostVersion => Get("LaunchState.LostVersion"),
            LaunchState.LostLib => Get("LaunchState.LostLib"),
            LaunchState.LostLoader => Get("LaunchState.LostLoader"),
            LaunchState.LostLoginCore => Get("LaunchState.LostLoginCore"),
            LaunchState.Download => Get("LaunchState.Download"),
            LaunchState.JvmPrepare => Get("LaunchState.JvmPrepare"),
            LaunchState.VersionError => Get("LaunchState.VersionError"),
            LaunchState.AssetsError => Get("LaunchState.AssetsError"),
            LaunchState.LoaderError => Get("LaunchState.LoaderError"),
            LaunchState.LostFile => Get("LaunchState.LostFile"),
            LaunchState.DownloadFail => Get("LaunchState.DownloadFail"),
            LaunchState.JavaError => Get("LaunchState.JvmError"),
            LaunchState.LaunchPre => Get("LaunchState.LaunchPre"),
            LaunchState.LaunchPost => Get("LaunchState.LaunchPost"),
            _ => Get("LaunchState.Other")
        };
    }

    public static string GetName(this GetDownloadState state)
    {
        return state switch
        {
            GetDownloadState.Init => Get("DownloadState.Init"),
            GetDownloadState.GetInfo => Get("DownloadState.GetInfo"),
            GetDownloadState.End => Get("DownloadState.End"),
            _ => Get("DownloadState.Other")
        };
    }

    public static string GetName(this DownloadItemState state)
    {
        return state switch
        {
            DownloadItemState.Wait => Get("DownloadItemState.Wait"),
            DownloadItemState.GetInfo => Get("DownloadItemState.GetInfo"),
            DownloadItemState.Download => Get("DownloadItemState.Download"),
            DownloadItemState.Init => Get("DownloadItemState.Init"),
            DownloadItemState.Pause => Get("DownloadItemState.Pause"),
            DownloadItemState.Action => Get("DownloadItemState.Action"),
            DownloadItemState.Done => Get("DownloadItemState.Done"),
            DownloadItemState.Error => Get("DownloadItemState.Error"),
            _ => Get("DownloadItemState.Other")
        };
    }

    public static string GetName(this CurseForgeSortField state)
    {
        return state switch
        {
            CurseForgeSortField.Featured => Get("CurseForgeSortField.Featured"),
            CurseForgeSortField.Popularity => Get("CurseForgeSortField.Popularity"),
            CurseForgeSortField.LastUpdated => Get("CurseForgeSortField.LastUpdated"),
            CurseForgeSortField.Name => Get("CurseForgeSortField.Name"),
            CurseForgeSortField.Author => Get("CurseForgeSortField.Author"),
            CurseForgeSortField.TotalDownloads => Get("CurseForgeSortField.TotalDownloads"),
            CurseForgeSortField.Category => Get("CurseForgeSortField.Category"),
            CurseForgeSortField.GameVersion => Get("CurseForgeSortField.GameVersion"),
            _ => Get("CurseForgeSortField.Other")
        };
    }

    public static string GetNameWithRelease(int type)
    {
        return type switch
        {
            1 => Get("Release.Release"),
            2 => Get("Release.Beta"),
            3 => Get("Release.Alpha"),
            _ => Get("Release.Other")
        };
    }

    public static string GetName(this SourceLocal state)
    {
        return state switch
        {
            SourceLocal.Offical => Get("SourceLocal.Offical"),
            SourceLocal.BMCLAPI => Get("SourceLocal.BMCLAPI"),
            SourceLocal.MCBBS => Get("SourceLocal.MCBBS"),
            _ => Get("SourceLocal.Other")
        };
    }

    public static string GetName(this GCType state)
    {
        return state switch
        {
            GCType.G1GC => Get("GCType.G1GC"),
            GCType.SerialGC => Get("GCType.SerialGC"),
            GCType.ParallelGC => Get("GCType.ParallelGC"),
            GCType.CMSGC => Get("GCType.CMSGC"),
            GCType.User => Get("GCType.User"),
            _ => Get("GCType.Other")
        };
    }

    public static string GetName(this ArchEnum arch)
    {
        return arch switch
        {
            ArchEnum.x64 => Get("ArchEnum.x64"),
            ArchEnum.x32 => Get("ArchEnum.x32"),
            _ => Get("ArchEnum.Other")
        };
    }

    public static string GetName(this OsType arch)
    {
        return arch switch
        {
            OsType.Windows => Get("OsType.Windows"),
            OsType.Linux => Get("OsType.Linux"),
            OsType.MacOS => Get("OsType.MacOS"),
            _ => Get("OsType.Other")
        };
    }

    public static string GetName(this LanguageType type)
    {
        return type switch
        {
            LanguageType.en_us => "English",
            LanguageType.zh_cn => "简体中文",
            _ => ""
        };
    }

    public static string GetName(this Loaders type)
    {
        return type switch
        {
            Loaders.Normal => "Normal",
            Loaders.Forge => "Forge",
            Loaders.Fabric => "Fabric",
            Loaders.Quilt => "Quilt",
            _ => "Unkown"
        };
    }

    public static string GetNameWithGameType(int type)
    {
        return type switch
        {
            0 => Get("GameType.Survival"),
            1 => Get("GameType.Creative"),
            2 => Get("GameType.Adventure"),
            3 => Get("GameType.Spectator"),
            _ => Get("GameType.Other")
        };
    }

    public static string GetNameWithDifficulty(int type)
    {
        return type switch
        {
            0 => Get("Difficulty.Peaceful"),
            1 => Get("Difficulty.Easy"),
            2 => Get("Difficulty.Normal"),
            3 => Get("Difficulty.Hard"),
            _ => Get("Difficulty.Other")
        };
    }

    public static string GetName(this SourceType type)
    {
        return type switch
        {
            SourceType.CurseForge => Get("SourceType.CurseForge"),
            SourceType.Modrinth => Get("SourceType.Modrinth"),
            SourceType.McMod => Get("SourceType.McMod"),
            _ => Get("SourceType.Other")
        };
    }

    public static string GetName(this PackType type)
    {
        return type switch
        {
            PackType.ColorMC => Get("PackType.ColorMC"),
            PackType.CurseForge => Get("PackType.CurseForge"),
            PackType.Modrinth => Get("PackType.Modrinth"),
            PackType.MMC => Get("PackType.MMC"),
            PackType.HMCL => Get("PackType.HMCL"),
            _ => Get("PackType.Other")
        };
    }

    public static string GetName(this FileType type)
    {
        return type switch
        {
            FileType.ModPack => Get("FileType.ModPack"),
            FileType.Mod => Get("FileType.Mod"),
            FileType.World => Get("FileType.World"),
            FileType.Shaderpack => Get("FileType.Shaderpack"),
            FileType.Resourcepack => Get("FileType.Resourcepack"),
            FileType.DataPacks => Get("FileType.DataPacks"),
            FileType.Optifne => Get("FileType.Optifne"),
            _ => Get("FileType.Other")
        };
    }
}
