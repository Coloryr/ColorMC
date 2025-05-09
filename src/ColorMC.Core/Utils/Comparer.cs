using System.Diagnostics.CodeAnalysis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Utils;

/// <summary>
/// 光影包比较器
/// </summary>
public class CustomGameArgObjComparer : IComparer<CustomGameArgObj>
{
    public static readonly CustomGameArgObjComparer Instance = new();
    public int Compare(CustomGameArgObj? x, CustomGameArgObj? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        return x.Order.CompareTo(y.Order);
    }
}

/// <summary>
/// 光影包比较器
/// </summary>
public class ShaderpackObjComparer : IComparer<ShaderpackObj>
{
    public static readonly ShaderpackObjComparer Instance = new();
    public int Compare(ShaderpackObj? x, ShaderpackObj? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        return y.Name.CompareTo(x.Name);
    }
}

/// <summary>
/// 结构体比较器
/// </summary>
public class SchematicObjComparer : IComparer<SchematicObj>
{
    public static readonly SchematicObjComparer Instance = new();
    public int Compare(SchematicObj? x, SchematicObj? y)
    {
        if (x == null || y == null || x.Name == null || y.Name == null)
        {
            return 0;
        }
        return y.Name.CompareTo(x.Name);
    }
}

/// <summary>
/// 数据包比较器
/// </summary>
public class DataPackObjComparer : IComparer<DataPackObj>
{
    public static readonly DataPackObjComparer Instance = new();
    public int Compare(DataPackObj? x, DataPackObj? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }
        return y.Name.CompareTo(x.Name);
    }
}

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
public class CurseForgeDataComparer : IEqualityComparer<CurseForgeModObj.CurseForgeDataObj>
{
    public static readonly CurseForgeDataComparer Instance = new();
    public bool Equals(CurseForgeModObj.CurseForgeDataObj? x, CurseForgeModObj.CurseForgeDataObj? y)
    {
        return x?.Id == y?.Id;
    }

    public int GetHashCode([DisallowNull] CurseForgeModObj.CurseForgeDataObj obj)
    {
        return obj.Id;
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

        var b3 = string.IsNullOrWhiteSpace(x.ModId);
        var b4 = string.IsNullOrWhiteSpace(y.ModId);
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
            return x.ModId.CompareTo(y.ModId);
        }
    }
}
