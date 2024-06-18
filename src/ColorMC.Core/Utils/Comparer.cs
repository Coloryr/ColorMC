using System.Diagnostics.CodeAnalysis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Utils;

/// <summary>
/// CurseForge比较器
/// </summary>
public class VersionStrObjComparer : IComparer<VersionStrObj>
{
    public static readonly VersionStrObjComparer Instance = new();
    public int Compare(VersionStrObj? x, VersionStrObj? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        return y.Version.CompareTo(x.Version);
    }
}

/// <summary>
/// CurseForge比较器
/// </summary>
public class CurseForgeDataComparer : IEqualityComparer<CurseForgeModObj.Data>
{
    public static readonly CurseForgeDataComparer Instance = new();
    public bool Equals(CurseForgeModObj.Data? x, CurseForgeModObj.Data? y)
    {
        return x?.id == y?.id;
    }

    public int GetHashCode([DisallowNull] CurseForgeModObj.Data obj)
    {
        return obj.id;
    }
}

/// <summary>
/// Mod比较器
/// </summary>
public class ModComparer : IComparer<ModObj>
{
    public static readonly ModComparer Instance = new();

    public int Compare(ModObj? x, ModObj? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        var b3 = string.IsNullOrWhiteSpace(x.name);
        var b4 = string.IsNullOrWhiteSpace(y.name);
        if (x == y)
        {
            return 0;
        }
        else if (x.ReadFail && y.ReadFail)
        {
            return x.Local.CompareTo(y.Local);
        }
        else if (x.ReadFail)
        {
            return -1;
        }
        else if (y.ReadFail)
        {
            return 1;
        }
        else if (b3)
        {
            return -1;
        }
        else if (b4)
        {
            return 1;
        }
        else if (b3 && b4)
        {
            return x.Local.CompareTo(y.Local);
        }
        else
        {
            return x.name.CompareTo(y.name);
        }
    }
}
