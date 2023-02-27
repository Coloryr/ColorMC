using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;
using System.Diagnostics.CodeAnalysis;

namespace ColorMC.Core.Utils;

public class CurseDataComparer : IEqualityComparer<CurseForgeModObj.Data>
{
    public static CurseDataComparer Instance = new();
    public bool Equals(CurseForgeModObj.Data? x, CurseForgeModObj.Data? y)
    {
        return x?.id == y?.id;
    }

    public int GetHashCode([DisallowNull] CurseForgeModObj.Data obj)
    {
        return obj.id;
    }
}

public class ModComparer : IComparer<ModObj>
{
    public static ModComparer Instance = new();

    public int Compare(ModObj? x, ModObj? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }
        else if (x == null || x.name == null)
        {
            return -1;
        }
        else if (y == null || y.name == null)
        {
            return 1;
        }
        if (x.name != y.name)
        {
            return x.name.CompareTo(y.name);
        }
        else return 0;
    }
}
