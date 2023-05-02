using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Java;
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

public record JavaInfoObj
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Info { get; set; }
}

public static class JavaBinding
{
    private readonly static List<string> JavaType =
        new() { "Adoptium", "Zulu", "Dragonwell", "OpenJ9" };

    private static JavaInfoObj MakeInfo(string name, JavaInfo item)
    {
        return new JavaInfoObj()
        {
            Name = name,
            Path = item.Path,
            Info = $"{item.Type} {item.Version} {item.Arch}"
        };
    }

    public static List<JavaInfoObj> GetJavaInfo()
    {
        List<JavaInfoObj> res = new();
        foreach (var item in JvmPath.Jvms)
        {
            res.Add(MakeInfo(item.Key, item.Value));
        }

        return res;
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
        var res = JvmPath.AddItem(name, local);
        if (res.Res == false)
        {
            return (null, res.Msg);
        }
        else
        {
            var info = JvmPath.GetInfo(res.Msg);
            if (info == null)
            {
                return (null, App.GetLanguage("Error5"));
            }
            return (MakeInfo(res.Msg, info), null);
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
        List<JavaDisplayObj> res = new();
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

    public static async Task<(bool, string?)> DownloadJava(JavaDownloadDisplayObj obj)
    {
        var res = await JvmPath.Install(obj.File, obj.Name, obj.Sha256, obj.Url);
        if (res.Item1 != CoreRunState.Init)
        {
            return (false, res.Item2);
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
            return new DirectoryInfo(JvmPath.BaseDir);

        return null;
    }

    public static void RemoveAllJava()
    {
        JvmPath.RemoveAll();
    }

    public static List<string> GetJavaType()
    {
        return JavaType;
    }

    public static async Task<(bool, List<string>? Arch, List<string>? Os, List<string>? MainVersion, List<JavaDownloadDisplayObj>?)> GetJavaList(int type, int os, int mainversion)
    {
        if (mainversion == -1)
            mainversion = 0;
        if (os == -1)
            os = 0;

        switch (type)
        {
            case 0:
                {
                    var res = await GetAdoptiumList(mainversion, os);
                    if (!res.Item1)
                    {
                        return (false, null, null, null, null);
                    }
                    else
                    {
                        return (true, res.Arch, Adoptium.SystemType, Adoptium.JavaVersion, res.Item3);
                    }
                }
            case 1:
                {
                    return await GetZuluList();
                }
            case 2:
                {
                    var res = await GetDragonwellList();
                    if (res == null)
                    {
                        return (false, null, null, null, null);
                    }
                    else
                    {
                        return (true, null, null, null, res);
                    }
                }
            case 3:
                {
                    return await GetOpenJ9List();
                }
            default:
                return (false, null, null, null, null);
        }
    }

    private static async Task<(bool, List<string>? Arch, List<string>? Os, List<string>? MainVersion,
        List<JavaDownloadDisplayObj>?)> GetZuluList()
    {
        try
        {
            var list = await Zulu.GetJavaList();
            if (list == null)
            {
                return (false, null, null, null, null);
            }

            var arch = new List<string>
            {
                ""
            };
            arch.AddRange(from item in list
                          group item by item.arch + '_' + item.hw_bitness into newGroup
                          orderby newGroup.Key descending
                          select newGroup.Key);

            var mainversion = new List<string>
            {
                ""
            };
            mainversion.AddRange(from item in list
                                 group item by item.java_version[0] into newGroup
                                 orderby newGroup.Key descending
                                 select newGroup.Key.ToString());

            var os = new List<string>
            {
                ""
            };
            os.AddRange(from item in list
                        group item by item.os into newGroup
                        orderby newGroup.Key descending
                        select newGroup.Key.ToString());

            var list1 = new List<JavaDownloadDisplayObj>();
            foreach (var item in list)
            {
                if (item.name.EndsWith(".deb") || item.name.EndsWith(".rpm") || item.name.EndsWith(".msi") || item.name.EndsWith(".dmg"))
                    continue;

                list1.Add(new()
                {
                    Name = item.name,
                    Arch = item.arch + '_' + item.hw_bitness,
                    Os = item.os,
                    MainVersion = item.zulu_version[0].ToString(),
                    Version = ToStr(item.zulu_version),
                    Size = UIUtils.MakeFileSize1(0),
                    Url = item.url,
                    Sha256 = item.sha256_hash,
                    File = item.name
                });
            }

            return (true, arch, os, mainversion, list1);
        }
        catch (Exception e)
        {
            App.ShowError(App.GetLanguage("JavaBinding.Error1"), e);
            return (false, null, null, null, null);
        }
    }

    private static string ToStr(List<int> list)
    {
        string a = "";
        foreach (var item in list)
        {
            a += item + ".";
        }
        return a[..^1];
    }

    private static async Task<(bool, List<string>? Arch, List<JavaDownloadDisplayObj>?)>
        GetAdoptiumList(int mainversion, int os)
    {
        try
        {
            var version = Adoptium.JavaVersion[mainversion];
            var list = await Adoptium.GetJavaList(version, os);
            if (list == null)
            {
                return (false, null, null);
            }

            var arch = new List<string>
            {
                ""
            };
            arch.AddRange(from item in list
                          group item by item.binary.architecture into newGroup
                          orderby newGroup.Key descending
                          select newGroup.Key);

            var list3 = new List<JavaDownloadDisplayObj>();
            foreach (var item in list)
            {
                if (item.binary.image_type == "debugimage")
                    continue;
                list3.Add(new()
                {
                    Name = item.binary.scm_ref + "_" + item.binary.image_type,
                    Arch = item.binary.architecture,
                    Os = item.binary.os,
                    MainVersion = version,
                    Version = item.version.openjdk_version,
                    Size = UIUtils.MakeFileSize1(item.binary.package.size),
                    Url = item.binary.package.link,
                    Sha256 = item.binary.package.checksum,
                    File = item.binary.package.name
                });
            }

            return (true, arch, list3);
        }
        catch (Exception e)
        {
            App.ShowError(App.GetLanguage("JavaBinding.Error1"), e);
            return (false, null, null);
        }
    }

    private static void AddDragonwell(List<JavaDownloadDisplayObj> list, DragonwellObj.Item item)
    {
        string main = "8";
        string version = item.version8;
        string file;
        if (item.xurl8 != null)
        {
            file = Path.GetFileName(item.xurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl8,
                File = file
            });
        }
        if (item.aurl8 != null)
        {
            file = Path.GetFileName(item.aurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl8,
                File = file
            });
        }
        if (item.wurl8 != null)
        {
            file = Path.GetFileName(item.wurl8);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl8,
                File = file
            });
        }

        main = "11";
        version = item.version11;
        if (item.xurl11 != null)
        {
            file = Path.GetFileName(item.xurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl11,
                File = file
            });
        }
        if (item.aurl11 != null)
        {
            file = Path.GetFileName(item.aurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl11,
                File = file
            });
        }
        if (item.apurl11 != null)
        {
            file = Path.GetFileName(item.apurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.apurl11,
                File = file
            });
        }
        if (item.wurl11 != null)
        {
            file = Path.GetFileName(item.wurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl11,
                File = file
            });
        }
        if (item.rurl11 != null)
        {
            file = Path.GetFileName(item.rurl11);
            list.Add(new()
            {
                Name = file,
                Arch = "riscv64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.rurl11,
                File = file
            });
        }

        main = "17";
        version = item.version17;
        if (item.xurl17 != null)
        {
            file = Path.GetFileName(item.xurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl17,
                File = file
            });
        }
        if (item.aurl17 != null)
        {
            file = Path.GetFileName(item.aurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl17,
                File = file
            });
        }
        if (item.apurl17 != null)
        {
            file = Path.GetFileName(item.apurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.apurl17,
                File = file
            });
        }
        if (item.wurl17 != null)
        {
            file = Path.GetFileName(item.wurl17);
            list.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl17,
                File = file
            });
        }
    }

    private static async Task<List<JavaDownloadDisplayObj>?> GetDragonwellList()
    {
        try
        {
            var list = await Dragonwell.GetJavaList();
            if (list == null)
            {
                return null;
            }

            var list1 = new List<JavaDownloadDisplayObj>();

            AddDragonwell(list1, list.extended);
            AddDragonwell(list1, list.standard);

            return list1;
        }
        catch (Exception e)
        {
            App.ShowError(App.GetLanguage("JavaBinding.Error1"), e);
            return null;
        }
    }

    private static async Task<(bool ok, List<string>? Arch, List<string>? Os,
        List<string>? MainVersion, List<JavaDownloadDisplayObj>?)> GetOpenJ9List()
    {
        try
        {
            var (Arch, Os, MainVersion, Data) = await OpenJ9.GetJavaList();
            if (Os == null)
            {
                return (false, null, null, null, null);
            }
            var list1 = new List<JavaDownloadDisplayObj>();

            foreach (var item in Data!)
            {
                var temp = item.name.Split("<br>");
                if (temp.Length != 3)
                {
                    continue;
                }
                var version = temp[0].Replace("<b>", "").Replace("</b>", "");
                if (item.jdk != null)
                    list1.Add(new()
                    {
                        Name = temp[2] + " " + temp[1] + "_jdk",
                        Os = item.os,
                        Arch = item.arch,
                        MainVersion = item.version.ToString(),
                        Version = version,
                        Size = "0",
                        Url = item.jdk.opt1.downloadLink,
                        Sha256 = item.jdk.opt1.checksum,
                        File = Path.GetFileName(item.jdk.opt1.downloadLink)
                    });
                if (item.jre != null)
                    list1.Add(new()
                    {
                        Name = temp[2] + " " + temp[1] + "_jre",
                        Os = item.os,
                        Arch = item.arch,
                        MainVersion = item.version.ToString(),
                        Version = version,
                        Size = "0",
                        Url = item.jre.opt1.downloadLink,
                        Sha256 = item.jre.opt1.checksum,
                        File = Path.GetFileName(item.jre.opt1.downloadLink)
                    });
            }

            return (true, Arch, Os, MainVersion, list1);
        }
        catch (Exception e)
        {
            App.ShowError(App.GetLanguage("JavaBinding.Error1"), e);
            return (false, null, null, null, null);
        }
    }

    public static List<JavaInfo>? FindJava()
    {
        return JavaHelper.FindJava();
    }
}
