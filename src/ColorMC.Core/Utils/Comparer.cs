using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;
using System.Diagnostics.CodeAnalysis;

namespace ColorMC.Core.Utils;

/// <summary>
/// CurseForge比较器
/// </summary>
public class CurseDataComparer : IEqualityComparer<CurseForgeModObj.Data>
{
    public readonly static CurseDataComparer Instance = new();
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
    public readonly static ModComparer Instance = new();

    public int Compare(ModObj? x, ModObj? y)
    {
        if (x == null || y == null)
            throw new Exception("ModObj is null");

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
