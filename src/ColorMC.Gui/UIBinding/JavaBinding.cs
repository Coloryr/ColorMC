using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Java;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class JavaBinding
{ 
    private static JavaInfoObj MakeInfo(string name, JavaInfo item)
    {
        return new JavaInfoObj()
        {
            Name = name,
            Path = item.Path,
            Info = $"{item.Type} {item.Version} {item.Arch}"
        };
    }

    public static async Task<(bool, string?)> AddJavaZip(string file, Action<string, int, int> zip)
    {
        string name = Path.GetFileName(file);
        if (SystemInfo.Os == OsType.Android)
        {
            name = name.Replace("primary%3A", "");
            name = name.Replace("%2F", "/");
            name = Path.GetFileName(name);
        }
        return await JvmPath.UnzipJavaAsync(name, file, zip);
    }

    public static List<string> GetJavaName()
    {
        var list = new List<string>();
        foreach (var item in JvmPath.Jvms)
        {
            list.Add(item.Key);
        }

        return list;
    }

    public static (JavaInfoObj?, string?) AddJava(string name, string local)
    {
        var (Res, Msg) = JvmPath.AddItem(name, local);
        if (Res == false)
        {
            return (null, Msg);
        }
        else
        {
            var info = JvmPath.GetInfo(Msg);
            if (info == null)
            {
                return (null, App.Lang("Gui.Error5"));
            }
            return (MakeInfo(Msg, info), null);
        }
    }

    public static JavaInfo? GetJavaInfo(string path)
    {
        return JavaHelper.GetJavaInfo(path);
    }

    public static void RemoveJava(string name)
    {
        JvmPath.Remove(name);
    }

    public static List<string> GetGCTypes()
    {
        var list = new List<string>()
        {
            "",
            GCType.G1GC.GetName(),
            GCType.SerialGC.GetName(),
            GCType.ParallelGC.GetName(),
            GCType.CMSGC.GetName(),
            GCType.User.GetName()
        };
        return list;
    }

    public static List<JavaDisplayObj> GetJavas()
    {
        var res = new List<JavaDisplayObj>();
        foreach (var item in JvmPath.Jvms)
        {
            res.Add(new()
            {
                Name = item.Key,
                Path = item.Value.Path,
                MajorVersion = item.Value.MajorVersion.ToString(),
                Version = item.Value.Version,
                Type = item.Value.Type,
                Arch = item.Value.Arch.GetName()
            });
        }

        return res;
    }

    public static async Task<(bool, string?)> DownloadJava(JavaDownloadObj obj)
    {
        var (Res, Message) = await JvmPath.InstallAsync(obj.File, obj.Name, obj.Sha256, obj.Url);
        if (Res != CoreRunState.Init)
        {
            return (false, Message);
        }

        return (true, null);
    }

    public static DirectoryInfo? GetSuggestedStartLocation()
    {
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                if (Directory.Exists("C:\\Program Files\\java"))
                    return new DirectoryInfo("C:\\Program Files\\java");
                else if (Directory.Exists("C:\\Program Files\\Java"))
                    return new DirectoryInfo("C:\\Program Files\\Java");
                break;
            case OsType.MacOS:
                if (Directory.Exists("/Library/Java/JavaVirtualMachines/"))
                    return new DirectoryInfo("/Library/Java/JavaVirtualMachines/");
                break;
        }

        if (Directory.Exists(JvmPath.BaseDir))
        {
            return new DirectoryInfo(JvmPath.BaseDir);
        }

        return null;
    }

    public static void RemoveAllJava()
    {
        JvmPath.RemoveAll();
    }

    public static List<JavaInfo>? FindJava()
    {
        return JavaHelper.FindJava();
    }
}
