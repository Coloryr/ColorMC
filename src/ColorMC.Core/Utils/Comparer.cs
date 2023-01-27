using ColorMC.Core.Objs.CurseForge;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
