using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;

namespace ColorMC.Core.Utils;

public enum LanguageType
{
    zh_cn, en_us
}

public static class LanguageHelper
{
    private static LanguageObj Languages;
    public static LanguageType Type;

    public static void Load(LanguageType type)
    {
        string name = type switch
        {
            LanguageType.en_us => "ColorMC.Core.Resources.Language.en-us.json",
            _ => "ColorMC.Core.Resources.Language.zh-cn.json"
        };
        Assembly assm = Assembly.GetExecutingAssembly();
        Stream istr = assm.GetManifestResourceStream(name);
        MemoryStream stream = new();
        istr?.CopyTo(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var array = stream.ToArray();
        var temp = Encoding.UTF8.GetString(array);
        try
        {
            Languages = JsonConvert.DeserializeObject<LanguageObj>(temp);
            Logs.Info(GetName("Language"));
        }
        catch (Exception e)
        {
            Logs.Error("语言文件读取错误", e);
        }
    }

    public static void Change(LanguageType type)
    {
        Type = type;
        Load(type);
        CoreMain.LanguageReload?.Invoke(type);
    }

    public static string GetName(string input)
    {
        if (Languages?.Language?.TryGetValue(input, out var temp) == true)
        {
            return temp;
        }
        return input;
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
            LoginState.ErrorType => GetName("LoginState.ErrorType"),
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
            LaunchState.JvmError => GetName("LaunchState.JvmError"),
            _ => GetName("LaunchState.Other")
        };
    }

    public static string GetName(this DownloadState state)
    {
        return state switch
        {
            DownloadState.Init => GetName("DownloadState.Init"),
            DownloadState.GetInfo => GetName("DownloadState.GetInfo"),
            DownloadState.End => GetName("DownloadState.End"),
            _ => GetName("DownloadState.Other")
        };
    }

    public static string GetName(this DownloadItemState state)
    {
        return state switch
        {
            DownloadItemState.Wait => GetName("DownloadItemState.Wait"),
            DownloadItemState.Download => GetName("DownloadItemState.Download"),
            DownloadItemState.Init => GetName("DownloadItemState.Init"),
            DownloadItemState.Action => GetName("DownloadItemState.Action"),
            DownloadItemState.Done => GetName("DownloadItemState.Done"),
            DownloadItemState.Error => GetName("DownloadItemState.Error"),
            _ => GetName("DownloadItemState.Other")
        };
    }

    public static string GetName(this SortField state)
    {
        return state switch
        {
            SortField.Featured => GetName("SortField.Featured"),
            SortField.Popularity => GetName("SortField.Popularity"),
            SortField.LastUpdated => GetName("SortField.LastUpdated"),
            SortField.Name => GetName("SortField.Name"),
            SortField.Author => GetName("SortField.Author"),
            SortField.TotalDownloads => GetName("SortField.TotalDownloads"),
            SortField.Category => GetName("SortField.Category"),
            SortField.GameVersion => GetName("SortField.GameVersion"),
            _ => GetName("SortField.Other")
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
            _ => "未知的语言"
        };
    }
}
