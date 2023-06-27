using ColorMC.Core.Helpers;

namespace ColorMC.Core.Objs.Modrinth;

public record MSortingObj
{
    public string Data { get; init; }
    public string GetName()
    {
        return LanguageHelper.Get($"MSortingType.{Data}");
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
