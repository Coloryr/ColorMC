using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using System.Reflection;

namespace ColorMC.Core.Helpers;

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
    public static string GetName(string input)
    {
        return Language.GetLanguage(input);
    }

    public static string GetName(this AuthType type)
    {
        return type switch
        {
            AuthType.Offline => GetName("AuthType.Offline"),
            AuthType.OAuth => GetName("AuthType.OAuth"),
            AuthType.Nide8 => GetName("AuthType.Nide8"),
            AuthType.AuthlibInjector => GetName("AuthType.AuthlibInjector"),
            AuthType.LittleSkin => GetName("AuthType.LittleSkin"),
            AuthType.SelfLittleSkin => GetName("AuthType.SelfLittleSkin"),
            _ => GetName("AuthType.Other")
        };
    }

    public static string GetName(this AuthState state)
    {
        return state switch
        {
            AuthState.OAuth => GetName("AuthType.Other"),
            AuthState.XBox => GetName("AuthType.XBox"),
            AuthState.XSTS => GetName("AuthType.XSTS"),
            AuthState.Token => GetName("AuthType.Token"),
            AuthState.Profile => GetName("AuthType.Profile"),
            _ => GetName("AuthType.Other")
        };
    }

    public static string GetName(this LoginState state)
    {
        return state switch
        {
            LoginState.Done => GetName("LoginState.Done"),
            LoginState.TimeOut => GetName("LoginState.TimeOut"),
            LoginState.JsonError => GetName("LoginState.JsonError"),
            LoginState.Error => GetName("LoginState.Error"),
            LoginState.Crash => GetName("LoginState.Crash"),
            _ => GetName("LoginState.Other")
        };
    }

    public static string GetName(this LaunchState state)
    {
        return state switch
        {
            LaunchState.Check => GetName("LaunchState.Check"),
            LaunchState.CheckVersion => GetName("LaunchState.CheckVersion"),
            LaunchState.CheckLib => GetName("LaunchState.CheckLib"),
            LaunchState.CheckAssets => GetName("LaunchState.CheckAssets"),
            LaunchState.CheckLoader => GetName("LaunchState.CheckLoader"),
            LaunchState.CheckLoginCore => GetName("LaunchState.CheckLoginCore"),
            LaunchState.LostVersion => GetName("LaunchState.LostVersion"),
            LaunchState.LostLib => GetName("LaunchState.LostLib"),
            LaunchState.LostLoader => GetName("LaunchState.LostLoader"),
            LaunchState.LostLoginCore => GetName("LaunchState.LostLoginCore"),
            LaunchState.Download => GetName("LaunchState.Download"),
            LaunchState.JvmPrepare => GetName("LaunchState.JvmPrepare"),
            LaunchState.VersionError => GetName("LaunchState.VersionError"),
            LaunchState.AssetsError => GetName("LaunchState.AssetsError"),
            LaunchState.LoaderError => GetName("LaunchState.LoaderError"),
            LaunchState.LostFile => GetName("LaunchState.LostFile"),
            LaunchState.DownloadFail => GetName("LaunchState.DownloadFail"),
            LaunchState.JavaError => GetName("LaunchState.JvmError"),
            LaunchState.LaunchPre => GetName("LaunchState.LaunchPre"),
            LaunchState.LaunchPost => GetName("LaunchState.LaunchPost"),
            _ => GetName("LaunchState.Other")
        };
    }

    public static string GetName(this GetDownloadState state)
    {
        return state switch
        {
            GetDownloadState.Init => GetName("DownloadState.Init"),
            GetDownloadState.GetInfo => GetName("DownloadState.GetInfo"),
            GetDownloadState.End => GetName("DownloadState.End"),
            _ => GetName("DownloadState.Other")
        };
    }

    public static string GetName(this DownloadItemState state)
    {
        return state switch
        {
            DownloadItemState.Wait => GetName("DownloadItemState.Wait"),
            DownloadItemState.GetInfo => GetName("DownloadItemState.GetInfo"),
            DownloadItemState.Download => GetName("DownloadItemState.Download"),
            DownloadItemState.Init => GetName("DownloadItemState.Init"),
            DownloadItemState.Pause => GetName("DownloadItemState.Pause"),
            DownloadItemState.Action => GetName("DownloadItemState.Action"),
            DownloadItemState.Done => GetName("DownloadItemState.Done"),
            DownloadItemState.Error => GetName("DownloadItemState.Error"),
            _ => GetName("DownloadItemState.Other")
        };
    }

    public static string GetName(this CurseForgeSortField state)
    {
        return state switch
        {
            CurseForgeSortField.Featured => GetName("CurseForgeSortField.Featured"),
            CurseForgeSortField.Popularity => GetName("CurseForgeSortField.Popularity"),
            CurseForgeSortField.LastUpdated => GetName("CurseForgeSortField.LastUpdated"),
            CurseForgeSortField.Name => GetName("CurseForgeSortField.Name"),
            CurseForgeSortField.Author => GetName("CurseForgeSortField.Author"),
            CurseForgeSortField.TotalDownloads => GetName("CurseForgeSortField.TotalDownloads"),
            CurseForgeSortField.Category => GetName("CurseForgeSortField.Category"),
            CurseForgeSortField.GameVersion => GetName("CurseForgeSortField.GameVersion"),
            _ => GetName("CurseForgeSortField.Other")
        };
    }

    public static string GetNameWithRelease(int type)
    {
        return type switch
        {
            1 => GetName("Release.Release"),
            2 => GetName("Release.Beta"),
            3 => GetName("Release.Alpha"),
            _ => GetName("Release.Other")
        };
    }

    public static string GetName(this SourceLocal state)
    {
        return state switch
        {
            SourceLocal.Offical => GetName("SourceLocal.Offical"),
            SourceLocal.BMCLAPI => GetName("SourceLocal.BMCLAPI"),
            SourceLocal.MCBBS => GetName("SourceLocal.MCBBS"),
            _ => GetName("SourceLocal.Other")
        };
    }

    public static string GetName(this GCType state)
    {
        return state switch
        {
            GCType.G1GC => GetName("GCType.G1GC"),
            GCType.SerialGC => GetName("GCType.SerialGC"),
            GCType.ParallelGC => GetName("GCType.ParallelGC"),
            GCType.CMSGC => GetName("GCType.CMSGC"),
            GCType.User => GetName("GCType.User"),
            _ => GetName("GCType.Other")
        };
    }

    public static string GetName(this ArchEnum arch)
    {
        return arch switch
        {
            ArchEnum.x64 => GetName("ArchEnum.x64"),
            ArchEnum.x32 => GetName("ArchEnum.x32"),
            _ => GetName("ArchEnum.Other")
        };
    }

    public static string GetName(this OsType arch)
    {
        return arch switch
        {
            OsType.Windows => GetName("OsType.Windows"),
            OsType.Linux => GetName("OsType.Linux"),
            OsType.MacOS => GetName("OsType.MacOS"),
            _ => GetName("OsType.Other")
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
            0 => GetName("GameType.Survival"),
            1 => GetName("GameType.Creative"),
            2 => GetName("GameType.Adventure"),
            3 => GetName("GameType.Spectator"),
            _ => GetName("GameType.Other")
        };
    }

    public static string GetNameWithDifficulty(int type)
    {
        return type switch
        {
            0 => GetName("Difficulty.Peaceful"),
            1 => GetName("Difficulty.Easy"),
            2 => GetName("Difficulty.Normal"),
            3 => GetName("Difficulty.Hard"),
            _ => GetName("Difficulty.Other")
        };
    }

    public static string GetName(this SourceType type)
    {
        return type switch
        {
            SourceType.CurseForge => GetName("SourceType.CurseForge"),
            SourceType.Modrinth => GetName("SourceType.Modrinth"),
            SourceType.McMod => GetName("SourceType.McMod"),
            _ => GetName("SourceType.Other")
        };
    }

    public static string GetName(this PackType type)
    {
        return type switch
        {
            PackType.ColorMC => GetName("PackType.ColorMC"),
            PackType.CurseForge => GetName("PackType.CurseForge"),
            PackType.Modrinth => GetName("PackType.Modrinth"),
            PackType.MMC => GetName("PackType.MMC"),
            PackType.HMCL => GetName("PackType.HMCL"),
            _ => GetName("PackType.Other")
        };
    }

    public static string GetName(this FileType type)
    {
        return type switch
        {
            FileType.ModPack => GetName("FileType.ModPack"),
            FileType.Mod => GetName("FileType.Mod"),
            FileType.World => GetName("FileType.World"),
            FileType.Shaderpack => GetName("FileType.Shaderpack"),
            FileType.Resourcepack => GetName("FileType.Resourcepack"),
            FileType.DataPacks => GetName("FileType.DataPacks"),
            FileType.Optifne => GetName("FileType.Optifne"),
            _ => GetName("FileType.Other")
        };
    }
}
