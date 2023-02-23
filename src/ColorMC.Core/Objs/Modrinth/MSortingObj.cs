using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Modrinth;

public record MSortingObj
{
    public string Data { get; init; }
    public string GetName()
    {
        return LanguageHelper.GetName($"MSortingType.{Data}");
    }

    public static readonly MSortingObj Relevance = new() { Data = "relevance" };
    public static readonly MSortingObj Downloads = new() { Data = "downloads" };
    public static readonly MSortingObj Follows = new() { Data = "follows" };
    public static readonly MSortingObj Newest = new() { Data = "newest" };
    public static readonly MSortingObj Updated = new() { Data = "updated" };

    public static List<string> NameList() => new()
    {
        Relevance.GetName(), 
        Downloads.GetName(), 
        Follows.GetName(), 
        Newest.GetName(), 
        Updated.GetName()
    };
}
