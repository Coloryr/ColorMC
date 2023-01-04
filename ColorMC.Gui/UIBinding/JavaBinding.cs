using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
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
}
