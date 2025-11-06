using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 语言文件
/// </summary>
public static class LanguageUtils
{
    private static readonly Dictionary<string, string> s_languageList = [];

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
            LanguageType.en_us => "ColorMC.Gui.Resources.Language.en-us.json",
            _ => "ColorMC.Gui.Resources.Language.zh-cn.json"
        };
        var assm = Assembly.GetExecutingAssembly();
        using var istr = assm.GetManifestResourceStream(name)!;
        s_languageList.Clear();
        var json = JsonDocument.Parse(istr);
        foreach (var item in json.RootElement.EnumerateObject())
        {
            s_languageList.Add(item.Name, item.Value.GetString()!);
        }
    }

    /// <summary>
    /// 切换语言文件
    /// </summary>
    /// <param name="type">语言列表</param>
    public static void Change(LanguageType type)
    {
        if (s_nowType == type)
        {
            return;
        }

        s_nowType = type;
        Load(type);
    }

    /// <summary>
    /// 取语言
    /// </summary>
    /// <param name="input">获取语言键</param>
    /// <returns>语言</returns>
    public static string Get(string input)
    {
        if (s_languageList.TryGetValue(input, out var res1))
        {
            return res1;
        }

        return input;
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

    public static string GetName(this DnsType type)
    {
        return type switch
        {
            DnsType.DnsOver => Get("Type.Dns.DnsOver"),
            DnsType.DnsOverHttps => Get("Type.Dns.DnsOverHttps"),
            _ => "Unkown"
        };
    }

    public static string GetName(this AuthState state)
    {
        return state switch
        {
            AuthState.OAuth => Get("Type.AuthState.OAuth"),
            AuthState.XBox => Get("Type.AuthState.XBox"),
            AuthState.XSTS => Get("Type.AuthState.XSTS"),
            AuthState.Token => Get("Type.AuthState.Token"),
            AuthState.Profile => Get("Type.AuthState.Profile"),
            _ => Get("Type.AuthState.Other")
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

    /// <summary>
    /// 获取错误信息
    /// </summary>
    /// <param name="exception">启动错误</param>
    /// <returns>信息</returns>
    public static string GetName(this LaunchException exception, GameSettingObj obj, LoginObj login)
    {
        return exception.State switch
        {
            LaunchError.LoginCoreError => string.Format(Get("Core.Error111"), login.AuthType.GetName(), login.UUID),
            LaunchError.LostVersionFile => string.Format(Get("Core.Error116"), obj.Version),
            LaunchError.LostLoaderFile => string.Format(Get("Core.Error104"), obj.Version, obj.Loader.GetName(), obj.LoaderVersion),
            LaunchError.LostAssetsFile => string.Format(Get("Core.Error103"), obj.Version, exception.InnerData),
            LaunchError.CheckServerPackError => Get("Core.Error14"),
            LaunchError.AuthLoginFail => string.Format(Get("Core.Error126"), login.AuthType.GetName(), login.UUID),
            LaunchError.DownloadFileError => Get("Core.Error106"),
            LaunchError.JavaNotFound => string.Format(Get("Core.Error107"), exception.InnerData),
            LaunchError.CmdFileNotFound => string.Format(Get("Core.Error113"), exception.InnerData),
            LaunchError.VersionError => Get("Core.Error108"),
            LaunchError.SelectJavaNotFound => Get("Core.Error112"),
            _ => "",
        };
    }

    public static string GetName(this GameSystemLog type, GameSettingObj game, GameLogItemObj obj)
    {
        return type switch
        {
            GameSystemLog.RuntimeLib => string.Format(Get("Core.Info28"), game.Name),
            GameSystemLog.JavaRedirect => string.Format(Get("Core.Info21"), game.Name),
            GameSystemLog.LoginTime => string.Format(Get("Core.Info30"), game.Name, obj.Category),
            GameSystemLog.ServerPackCheckTime => string.Format(Get("Core.Info39"), game.Name, obj.Category),
            GameSystemLog.CheckGameFileTime => string.Format(Get("Core.Info31"), game.Name, obj.Category),
            GameSystemLog.DownloadFileTime => string.Format(Get("Core.Info33"), game.Name, obj.Category),
            GameSystemLog.LaunchArgs => string.Format(Get("Core.Info27"), game.Name),
            GameSystemLog.JavaPath => string.Format(Get("Core.Info29"), game.Name, obj.Category),
            GameSystemLog.LaunchTime => string.Format(Get("Core.Info32"), game.Name, obj.Category),
            GameSystemLog.CmdPreTime => string.Format(Get("Core.Info34"), game.Name, obj.Category),
            GameSystemLog.CmdPostTime => string.Format(Get("Core.Info35"), game.Name, obj.Category),
            _ => ""
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
            GCType.Auto => Get("Type.GCType.Auto"),
            GCType.G1GC => Get("Type.GCType.G1GC"),
            GCType.ZGC => Get("Type.GCType.ZGC"),
            GCType.None => Get("Type.GCType.None"),
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

    public static string GetNameFail(this AuthState state)
    {
        return state switch
        {
            AuthState.OAuth => Get("Core.Error62"),
            AuthState.XBox => Get("Core.Error63"),
            AuthState.XSTS => Get("Core.Error64"),
            AuthState.Token => Get("Core.Error65"),
            AuthState.Profile => Get("Core.Error66"),
            _ => ""
        };
    }

    /// <summary>
    /// 获取登录错误信息
    /// </summary>
    /// <param name="state">登录错误</param>
    /// <returns>信息</returns>
    public static string GetName(this LoginFailState state)
    {
        return state switch
        {
            LoginFailState.GetOAuthCodeDataFail => Get("Core.Error121"),
            LoginFailState.GetOAuthCodeDataError => Get("Core.Error81"),
            LoginFailState.OAuthGetTokenTimeout => Get("Core.Error122"),
            LoginFailState.LoginAuthListEmpty => Get("Core.Error123"),
            LoginFailState.LoginTokenTimeout => Get("Core.Error124"),
            _ => "",
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

    public static string GetName(this ErrorType type)
    {
        return type switch
        {
            ErrorType.FileNotExist => Get("Core.Error137"),
            ErrorType.FileReadError => Get("Core.Error138"),
            ErrorType.DonwloadFail => Get("Core.Error139"),
            ErrorType.InstallFail => Get("Core.Error140"),
            ErrorType.UnzipFail => Get("Core.Error141"),
            ErrorType.SearchFail => Get("Core.Error142"),
            _ => Get("Core.Error136")
        };
    }
}
