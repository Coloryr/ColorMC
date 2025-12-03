using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;

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
            LanguageType.en_us => "ColorMC.Gui.Resource.Language.en-us.json",
            _ => "ColorMC.Gui.Resource.Language.zh-cn.json"
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

    public static string GetName(this AddState state)
    {
        return state switch
        {
            AddState.DownloadPack => Get("Type.ModpackState.Item6"),
            AddState.ReadInfo => Get("Type.ModpackState.Item1"),
            AddState.GetInfo => Get("Type.ModpackState.Item2"),
            AddState.DownloadFile => Get("Type.ModpackState.Item3"),
            AddState.Unzip => Get("Type.ModpackState.Item4"),
            AddState.Done => Get("Type.ModpackState.Item5"),
            _ => ""
        };
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
            LaunchError.LoginCoreError => string.Format(Get("Game.Error.Log5"), obj.UUID, login.AuthType.GetName(), login.UUID),
            LaunchError.LostVersionFile => string.Format(Get("Game.Error.Log6"), obj.UUID, obj.Version),
            LaunchError.LostLoaderFile => string.Format(Get("Game.Error.Log7"), obj.UUID, obj.Version, obj.Loader.GetName(), obj.LoaderVersion),
            LaunchError.LostAssetsFile => string.Format(Get("Game.Error.Log8"), obj.UUID, obj.Version, exception.InnerData),
            LaunchError.CheckServerPackError => string.Format(Get("Game.Error.Log2"), obj.UUID),
            LaunchError.AuthLoginFail => string.Format(Get("Game.Error.Log9"), obj.UUID, login.AuthType.GetName(), login.UUID),
            LaunchError.DownloadFileError => string.Format(Get("Game.Error.Log1"), obj.UUID),
            LaunchError.JavaNotFound => string.Format(Get("Game.Error.Log3"), obj.UUID, exception.InnerData),
            LaunchError.CmdFileNotFound => string.Format(Get("Game.Error.Log10"), obj.UUID, exception.InnerData),
            LaunchError.VersionError => string.Format(Get("Game.Error.Log4"), obj.UUID),
            LaunchError.SelectJavaNotFound => string.Format(Get("Game.Error.Log11"), obj.UUID, exception.InnerData),
            _ => "",
        };
    }

    public static string GetName(this GameSystemLog type, GameSettingObj game, GameLogItemObj obj)
    {
        return type switch
        {
            GameSystemLog.RuntimeLib => string.Format(Get("Game.Log3"), game.Name),
            GameSystemLog.JavaRedirect => string.Format(Get("Game.Log1"), game.Name),
            GameSystemLog.LoginTime => string.Format(Get("Game.Log5"), game.Name, obj.Category),
            GameSystemLog.ServerPackCheckTime => string.Format(Get("Game.Log11"), game.Name, obj.Category),
            GameSystemLog.CheckGameFileTime => string.Format(Get("Game.Log6"), game.Name, obj.Category),
            GameSystemLog.DownloadFileTime => string.Format(Get("Game.Log8"), game.Name, obj.Category),
            GameSystemLog.LaunchArgs => string.Format(Get("Game.Log2"), game.Name),
            GameSystemLog.JavaPath => string.Format(Get("Game.Log4"), game.Name, obj.Category),
            GameSystemLog.LaunchTime => string.Format(Get("Game.Log7"), game.Name, obj.Category),
            GameSystemLog.CmdPreTime => string.Format(Get("Game.Log9"), game.Name, obj.Category),
            GameSystemLog.CmdPostTime => string.Format(Get("Game.Log10"), game.Name, obj.Category),
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
            AuthState.OAuth => Get("Core.Error.Log13"),
            AuthState.XBox => Get("Core.Error.Log14"),
            AuthState.XSTS => Get("Core.Error.Log15"),
            AuthState.Token => Get("Core.Error.Log16"),
            AuthState.Profile => Get("Core.Error.Log17"),
            _ => ""
        };
    }

    /// <summary>
    /// 获取登录错误信息
    /// </summary>
    /// <param name="state">登录错误</param>
    /// <returns>信息</returns>
    public static string GetName(this LoginFailState state, AuthType type, string uuid)
    {
        return state switch
        {
            LoginFailState.GetDataFail => string.Format(Get("Core.Error.Log36"), type.GetName(), uuid),
            LoginFailState.GetDataError => string.Format(Get("Core.Error.Log18"), type.GetName(), uuid),
            LoginFailState.OAuthGetTokenTimeout => string.Format(Get("Core.Error.Log37"), type.GetName(), uuid),
            LoginFailState.LoginAuthListEmpty => Get("Core.Error.Log38"),
            LoginFailState.LoginTokenTimeout => Get("Core.Error.Log39"),
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
            FileType.Save => Get("Type.FileType.World"),
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
            ErrorType.FileNotExist => Get("Core.Error.Text1"),
            ErrorType.FileReadError => Get("Core.Error.Text2"),
            ErrorType.DonwloadFail => Get("Core.Error.Text3"),
            ErrorType.InstallFail => Get("Core.Error.Text4"),
            ErrorType.UnzipFail => Get("Core.Error.Text5"),
            ErrorType.SearchFail => Get("Core.Error.Text6"),
            _ => Get("Core.Error.Log43")
        };
    }

    public static string[] GetDisplayList()
    {
        return
        [
            Get("Type.DisplayType.Item1"),
            Get("Type.DisplayType.Item2"),
            Get("Type.DisplayType.Item3"),
            Get("Type.DisplayType.Item4"),
        ];
    }

    public static string[] GetLockLoginType()
    {
        return
        [
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        ];
    }

    public static string[] GetLoginUserType()
    {
        return
        [
            AuthType.Offline.GetName(),
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        ];
    }

    public static string[] GetDisplayUserTypes()
    {
        return
        [
            "",
            AuthType.Offline.GetName(),
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        ];
    }

    public static string[] GetGCTypes()
    {
        return
        [
            GCType.Auto.GetName(),
            GCType.G1GC.GetName(),
            GCType.ZGC.GetName(),
            GCType.None.GetName()
        ];
    }

    public static string[] GetAxisTypeName()
    {
        return
        [
            Get("Type.AxisType.Item1"),
            Get("Type.AxisType.Item2")
        ];
    }

    /// <summary>
    /// 获取过滤器选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetFilterName()
    {
        return
        [
            Get("Text.Name"),
            Get("Text.FileName"),
            Get("Text.Author"),
            "modid",
            Get("GameEditWindow.Tab4.Text16"),
            Get("GameEditWindow.Tab4.Text17"),
            Get("GameEditWindow.Tab4.Text18"),
        ];
    }

    public static string[] GetExportName()
    {
        return
        [
            Get("Type.PackType.ColorMC"),
            Get("Type.PackType.CurseForge"),
            Get("Type.PackType.Modrinth"),
        ];
    }

    public static string[] GetSkinType()
    {
        return
        [
            Get("Type.SkinType.Old"),
            Get("Type.SkinType.New"),
            Get("Type.SkinType.New_Slim")
        ];
    }
    /// <summary>
    /// 获取旋转选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetSkinRotateName()
    {
        return
        [
            Get("Type.SkinRotate.Item1"),
            Get("Type.SkinRotate.Item2"),
            Get("Type.SkinRotate.Item3")
        ];
    }

    /// <summary>
    /// 获取下载源选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetDownloadSources()
    {
        return
        [
            SourceLocal.Offical.GetName(),
            SourceLocal.BMCLAPI.GetName()
        ];
    }

    public static string[] GetDns()
    {
        return
        [
            Get("Type.Dns.Item1"),
            Get("Type.Dns.Item2"),
            Get("Type.Dns.Item3")
        ];
    }

    /// <summary>
    /// 获取窗口透明选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetWindowTranTypes()
    {
        return
        [
            Get("Type.TranTypes.Item1"),
            Get("Type.TranTypes.Item2"),
            Get("Type.TranTypes.Item3"),
            Get("Type.TranTypes.Item4"),
            Get("Type.TranTypes.Item5")
        ];
    }
    /// <summary>
    /// 获取语言选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetLanguages()
    {
        return
        [
            LanguageType.zh_cn.GetName(),
            LanguageType.en_us.GetName()
        ];
    }

    public static string[] GetCurseForgeSortTypes()
    {
        return
        [
            CurseForgeSortField.Featured.GetName(),
            CurseForgeSortField.Popularity.GetName(),
            CurseForgeSortField.LastUpdated.GetName(),
            CurseForgeSortField.Name.GetName(),
            CurseForgeSortField.TotalDownloads.GetName()
        ];
    }

    public static string[] GetModrinthSortTypes()
    {
        return
        [
            ModrinthHelper.Relevance.GetName(),
            ModrinthHelper.Downloads.GetName(),
            ModrinthHelper.Follows.GetName(),
            ModrinthHelper.Newest.GetName(),
            ModrinthHelper.Updated.GetName()
        ];
    }

    public static string[] GetSortOrder()
    {
        return
        [
            Get("Type.SortOrder.Item1"),
            Get("Type.SortOrder.Item2")
        ];
    }

    public static string[] GetSourceList()
    {
        return
        [
            SourceType.CurseForge.GetName(),
            SourceType.Modrinth.GetName()
        ];
    }

    public static string[] GetPackType()
    {
        return
        [
            PackType.ColorMC.GetName(),
            PackType.CurseForge.GetName(),
            PackType.Modrinth.GetName(),
            PackType.MMC.GetName(),
            PackType.HMCL.GetName(),
            PackType.ZipPack.GetName()
        ];
    }

    public static string[] GetAddType()
    {
        return
        [
            FileType.Mod.GetName(),
            FileType.Save.GetName(),
            FileType.Shaderpack.GetName(),
            FileType.Resourcepack.GetName(),
            FileType.DataPacks.GetName(),
            FileType.Optifine.GetName()
        ];
    }

    public static string[] GetNbtName()
    {
        return
        [
            "NbtEnd",
            "NbtByte",
            "NbtShort",
            "NbtInt",
            "NbtLong",
            "NbtFloat",
            "NbtDouble",
            "NbtByteArray",
            "NbtString",
            "NbtList",
            "NbtCompound",
            "NbtIntArray",
            "NbtLongArray",
        ];
    }

    public static string[] GetPCJavaType()
    {
        return
        [
            "Adoptium",
            "Zulu",
            "Dragonwell",
            "OpenJ9",
        ];
    }

    public static string[] GetFuntionList()
    {
        return
        [
            Get("ServerPackWindow.Tab4.Text5"),
            Get("ServerPackWindow.Tab4.Text6")
        ];
    }

    public static string[] GetVersionType()
    {
        return
        [
            Get("Type.VersionType.Item1"),
            Get("Type.VersionType.Item2"),
            Get("Type.VersionType.Item3")
        ];
    }

    public static string[] GetPos()
    {
        return
        [
            Get("Type.Postion.Item1"),
            Get("Type.Postion.Item2"),
            Get("Type.Postion.Item3"),
            Get("Type.Postion.Item4"),
            Get("Type.Postion.Item5"),
            Get("Type.Postion.Item6"),
            Get("Type.Postion.Item7"),
            Get("Type.Postion.Item8"),
            Get("Type.Postion.Item9"),
        ];
    }

    public static string[] GetGuide()
    {
        return
        [
            Get("Type.Guide.Item1"),
            Get("Type.Guide.Item2"),
        ];
    }

    public static string[] GetLoader()
    {
        return
        [
            Loaders.Forge.GetName(),
            Loaders.Fabric.GetName(),
            Loaders.Quilt.GetName(),
            Loaders.NeoForge.GetName(),
        ];
    }
}
