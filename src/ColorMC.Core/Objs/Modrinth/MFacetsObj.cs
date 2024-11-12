using System.Text;
using ColorMC.Core.Helpers;

namespace ColorMC.Core.Objs.Modrinth;

public record MFacetsObj
{
    public string Data { get; init; }
    public List<string> Values = [];
}
