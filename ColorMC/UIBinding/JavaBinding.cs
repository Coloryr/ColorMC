using ColorMC.Core.Path;
using System.Collections.Generic;

namespace ColorMC.UIBinding;

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

    public static (JavaInfoObj, string) AddJava(string name, string local)
    {
        var res = JvmPath.AddItem(name, local);
        if (res.Item1 == false)
        {
            return (null, res.Item2);
        }
        else
        {
            var info = JvmPath.GetInfo(res.Item2);
            return (MakeInfo(res.Item2, info), null);
        }
    }
}
