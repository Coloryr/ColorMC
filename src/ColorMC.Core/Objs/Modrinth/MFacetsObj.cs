using ColorMC.Core.Helpers;
using System.Text;

namespace ColorMC.Core.Objs.Modrinth;

public record MFacetsObj
{
    public string Data { get; init; }
    public List<string> Values = new();

    public string GetName()
    {
        return LanguageHelper.GetName($"MFacets.{Data}");
    }

    public static string Build(List<MFacetsObj> list)
    {
        var builder = new StringBuilder();
        builder.Append("[");
        foreach (var item in list)
        {
            if (item.Values.Count == 0)
                continue;

            foreach (var item1 in item.Values)
            {
                builder.Append($"[\"{item.Data}:{item1}\"],");
            }

        }
        builder.Remove(builder.Length - 1, 1);
        builder.Append("]");
        return builder.ToString();
    }

    public static MFacetsObj BuildCategories(List<string> values) => new()
    {
        Data = Categories.Data,
        Values = values
    };

    public static MFacetsObj BuildVersions(List<string> values) => new()
    {
        Data = Versions.Data,
        Values = values
    };

    public static MFacetsObj BuildProjectType(List<string> values) => new()
    {
        Data = ProjectType.Data,
        Values = values
    };

    private static MFacetsObj Categories = new()
    {
        Data = "categories"
    };
    private static MFacetsObj Versions = new()
    {
        Data = "versions"
    };
    private static MFacetsObj ProjectType = new()
    {
        Data = "project_type"
    };

    public static List<string> NameList() => new()
    {
        Categories.GetName(),
        Versions.GetName(),
        ProjectType.GetName()
    };
}
