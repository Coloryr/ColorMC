using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UIBinding;

public record JavaInfoObj
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Info { get; set; }
}

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
                return (null, "Java找不到");
            }
            return (MakeInfo(res.Msg, info), null);
        }
    }

    public static void RemoveJava(string name)
    {
        JvmPath.Remove(name);
    }

    public static List<string> GetGCTypes()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(GCType));
        foreach (GCType value in values)
        {
            list.Add(value.GetName());
        }

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
}
