using System.Reflection;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 语言文件
/// </summary>
public static class LanguageHelper
{
    /// <summary>
    /// 语言储存
    /// </summary>
    private static readonly Language s_language = new();
    /// <summary>
    /// 语言类型
    /// </summary>
    private static LanguageType s_nowType;

    /// <summary>
    /// 加载语言文件
    /// </summary>
    /// <param name="type">语言列表</param>
    public static void Load(LanguageType type)
    {
        string name = type switch
        {
            LanguageType.en_us => "ColorMC.Core.Resources.Language.core_en-us.json",
            _ => "ColorMC.Core.Resources.Language.core_zh-cn.json"
        };
        var assm = Assembly.GetExecutingAssembly();
        using var istr = assm.GetManifestResourceStream(name)!;
        s_language.Load(istr);
    }

    /// <summary>
    /// 切换语言文件
    /// </summary>
    /// <param name="type">语言列表</param>
    public static void Change(LanguageType type)
    {
        if (s_nowType == type)
            return;

        s_nowType = type;
        Load(type);
        ColorMCCore.OnLanguageReload(type);
    }

    /// <summary>
    /// 取语言
    /// </summary>
    /// <param name="input">获取语言键</param>
    /// <returns>语言</returns>
    public static string Get(string input)
    {
        return s_language.GetLanguage(input);
    }

    public static string GetName(this MFacetsObj type)
    {
        return Get($"Type.Modrinth.Facets.{type.Data}");
    }

    public static string GetName(this MSortingObj type)
    {
        return Get($"Type.Modrinth.SortingType.{type.Data}");
    }

    public static string GetName(this AuthType type)
    {
        return type switch
        {
            AuthType.Offline => Get("Type.AuthType.Offline"),
            AuthType.OAuth => Get("Type.AuthType.OAuth"),
            AuthType.Nide8 => Get("Type.AuthType.Nide8"),
            AuthType.AuthlibInjector => Get("Type.AuthType.AuthlibInjector"),
            AuthType.LittleSkin => Get("Type.AuthType.LittleSkin"),
            AuthType.SelfLittleSkin => Get("Type.AuthType.SelfLittleSkin"),
            _ => Get("Type.AuthType.Other")
        };
    }

    public static string GetName(this AuthState state)
    {
        return state switch
        {
            AuthState.OAuth => Get("Type.AuthType.Other"),
            AuthState.XBox => Get("Type.AuthType.XBox"),
            AuthState.XSTS => Get("Type.AuthType.XSTS"),
            AuthState.Token => Get("Type.AuthType.Token"),
            AuthState.Profile => Get("Type.AuthType.Profile"),
            _ => Get("Type.AuthType.Other")
        };
    }

    public static string GetName(this SideType state)
    {
        return state switch
        {
            SideType.None => Get("Type.SideType.None"),
            SideType.Client => Get("Type.SideType.Client"),
            SideType.Server => Get("Type.SideType.Server"),
            _ => Get("Type.SideType.Both")
        };
    }

    public static string GetName(this LoginState state)
    {
        return state switch
        {
            LoginState.Done => Get("Type.LoginState.Done"),
            LoginState.TimeOut => Get("Type.LoginState.TimeOut"),
            LoginState.DataError => Get("Type.LoginState.DataError"),
            LoginState.Error => Get("Type.LoginState.Error"),
            LoginState.Crash => Get("Type.LoginState.Crash"),
            _ => Get("Type.LoginState.Other")
        };
    }

    public static string GetName(this LaunchState state)
    {
        return state switch
        {
            LaunchState.Check => Get("Type.LaunchState.Check"),
            LaunchState.CheckVersion => Get("Type.LaunchState.CheckVersion"),
            LaunchState.CheckLib => Get("Type.LaunchState.CheckLib"),
            LaunchState.CheckAssets => Get("Type.LaunchState.CheckAssets"),
            LaunchState.CheckLoader => Get("Type.LaunchState.CheckLoader"),
            LaunchState.CheckLoginCore => Get("Type.LaunchState.CheckLoginCore"),
            LaunchState.LostVersion => Get("Type.LaunchState.LostVersion"),
            LaunchState.LostLib => Get("Type.LaunchState.LostLib"),
            LaunchState.LostLoader => Get("Type.LaunchState.LostLoader"),
            LaunchState.LostLoginCore => Get("Type.LaunchState.LostLoginCore"),
            LaunchState.Download => Get("Type.LaunchState.Download"),
            LaunchState.JvmPrepare => Get("Type.LaunchState.JvmPrepare"),
            LaunchState.VersionError => Get("Type.LaunchState.VersionError"),
            LaunchState.AssetsError => Get("Type.LaunchState.AssetsError"),
            LaunchState.LoaderError => Get("Type.LaunchState.LoaderError"),
            LaunchState.LostFile => Get("Type.LaunchState.LostFile"),
            LaunchState.DownloadFail => Get("Type.LaunchState.DownloadFail"),
            LaunchState.JavaError => Get("Type.LaunchState.JvmError"),
            LaunchState.LaunchPre => Get("Type.LaunchState.LaunchPre"),
            LaunchState.LaunchPost => Get("Type.LaunchState.LaunchPost"),
            _ => Get("Type.LaunchState.Other")
        };
    }

    public static string GetName(this GetDownloadState state)
    {
        return state switch
        {
            GetDownloadState.Init => Get("Type.DownloadState.Init"),
            GetDownloadState.GetInfo => Get("Type.DownloadState.GetInfo"),
            GetDownloadState.End => Get("Type.DownloadState.End"),
            _ => Get("Type.DownloadState.Other")
        };
    }

    public static string GetName(this DownloadItemState state)
    {
        return state switch
        {
            DownloadItemState.Wait => Get("Type.DownloadItemState.Wait"),
            DownloadItemState.GetInfo => Get("Type.DownloadItemState.GetInfo"),
            DownloadItemState.Download => Get("Type.DownloadItemState.Download"),
            DownloadItemState.Init => Get("Type.DownloadItemState.Init"),
            DownloadItemState.Pause => Get("Type.DownloadItemState.Pause"),
            DownloadItemState.Action => Get("Type.DownloadItemState.Action"),
            DownloadItemState.Done => Get("Type.DownloadItemState.Done"),
            DownloadItemState.Error => Get("Type.DownloadItemState.Error"),
            _ => Get("Type.DownloadItemState.Other")
        };
    }

    public static string GetName(this CurseForgeSortField state)
    {
        return state switch
        {
            CurseForgeSortField.Featured => Get("Type.CurseForge.SortField.Featured"),
            CurseForgeSortField.Popularity => Get("Type.CurseForge.SortField.Popularity"),
            CurseForgeSortField.LastUpdated => Get("Type.CurseForge.SortField.LastUpdated"),
            CurseForgeSortField.Name => Get("Type.CurseForge.SortField.Name"),
            CurseForgeSortField.Author => Get("Type.CurseForge.SortField.Author"),
            CurseForgeSortField.TotalDownloads => Get("Type.CurseForge.SortField.TotalDownloads"),
            CurseForgeSortField.Category => Get("Type.CurseForge.SortField.Category"),
            CurseForgeSortField.GameVersion => Get("Type.CurseForge.SortField.GameVersion"),
            _ => Get("Type.CurseForge.SortField.Other")
        };
    }

    public static string GetNameWithRelease(int type)
    {
        return type switch
        {
            1 => Get("Type.Release.Release"),
            2 => Get("Type.Release.Beta"),
            3 => Get("Type.Release.Alpha"),
            _ => Get("Type.Release.Other")
        };
    }

    public static string GetName(this SourceLocal state)
    {
        return state switch
        {
            SourceLocal.Offical => Get("Type.SourceLocal.Offical"),
            SourceLocal.BMCLAPI => Get("Type.SourceLocal.BMCLAPI"),
            _ => Get("Type.SourceLocal.Other")
        };
    }

    public static string GetName(this GCType state)
    {
        return state switch
        {
            GCType.G1GC => Get("Type.GCType.G1GC"),
            GCType.SerialGC => Get("Type.GCType.SerialGC"),
            GCType.ParallelGC => Get("Type.GCType.ParallelGC"),
            GCType.CMSGC => Get("Type.GCType.CMSGC"),
            GCType.User => Get("Type.GCType.User"),
            _ => Get("Type.GCType.Other")
        };
    }

    public static string GetName(this ArchEnum arch)
    {
        return arch switch
        {
            ArchEnum.x86_64 => Get("Type.ArchEnum.x64"),
            ArchEnum.x86 => Get("Type.ArchEnum.x32"),
            ArchEnum.aarch64 => Get("Type.ArchEnum.aarch64"),
            ArchEnum.arm => Get("Type.ArchEnum.armV7"),
            _ => Get("Type.ArchEnum.Other")
        };
    }

    public static string GetName(this OsType arch)
    {
        return arch switch
        {
            OsType.Windows => Get("Type.OsType.Windows"),
            OsType.Linux => Get("Type.OsType.Linux"),
            OsType.MacOS => Get("Type.OsType.MacOS"),
            _ => Get("Type.OsType.Other")
        };
    }

    public static string GetName(this LanguageType type)
    {
        return type switch
        {
            LanguageType.en_us => "English(AI)",
            LanguageType.zh_cn => "简体中文",
            _ => ""
        };
    }

    public static string GetName(this Loaders type)
    {
        return type switch
        {
            Loaders.Normal => Get("Type.Loaders.Normal"),
            Loaders.Forge => "Forge",
            Loaders.NeoForge => "NeoForge",
            Loaders.Fabric => "Fabric",
            Loaders.Quilt => "Quilt",
            Loaders.OptiFine => "OptiFine",
            Loaders.Custom => Get("Type.Loaders.Custom"),
            _ => "Unkown"
        };
    }

    public static string GetNameWithGameType(int type)
    {
        return type switch
        {
            0 => Get("Type.GameType.Survival"),
            1 => Get("Type.GameType.Creative"),
            2 => Get("Type.GameType.Adventure"),
            3 => Get("Type.GameType.Spectator"),
            _ => Get("Type.GameType.Other")
        };
    }

    public static string GetNameWithDifficulty(int type)
    {
        return type switch
        {
            0 => Get("Type.Difficulty.Peaceful"),
            1 => Get("Type.Difficulty.Easy"),
            2 => Get("Type.Difficulty.Normal"),
            3 => Get("Type.Difficulty.Hard"),
            _ => Get("Type.Difficulty.Other")
        };
    }

    public static string GetName(this SourceType type)
    {
        return type switch
        {
            SourceType.CurseForge => Get("Type.SourceType.CurseForge"),
            SourceType.Modrinth => Get("Type.SourceType.Modrinth"),
            SourceType.McMod => Get("Type.SourceType.McMod"),
            _ => Get("Type.SourceType.Other")
        };
    }

    public static string GetName(this PackType type)
    {
        return type switch
        {
            PackType.ColorMC => Get("Type.PackType.ColorMC"),
            PackType.CurseForge => Get("Type.PackType.CurseForge"),
            PackType.Modrinth => Get("Type.PackType.Modrinth"),
            PackType.MMC => Get("Type.PackType.MMC"),
            PackType.HMCL => Get("Type.PackType.HMCL"),
            PackType.ZipPack => Get("Type.PackType.ZipPack"),
            _ => Get("Type.PackType.Other")
        };
    }

    public static string GetName(this FileType type)
    {
        return type switch
        {
            FileType.ModPack => Get("Type.FileType.ModPack"),
            FileType.Mod => Get("Type.FileType.Mod"),
            FileType.World => Get("Type.FileType.World"),
            FileType.Shaderpack => Get("Type.FileType.Shaderpack"),
            FileType.Resourcepack => Get("Type.FileType.Resourcepack"),
            FileType.DataPacks => Get("Type.FileType.DataPacks"),
            FileType.Optifine => Get("Type.FileType.Optifne"),
            _ => Get("Type.FileType.Other")
        };
    }
}
