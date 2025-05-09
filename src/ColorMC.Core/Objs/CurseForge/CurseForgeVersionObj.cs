using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF 版本数据
/// </summary>
public record CurseForgeVersionObj
{
    public record CurseForgeVersionDataObj
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
        [JsonPropertyName("versions")]
        public List<string> Versions { get; set; }
    }

    [JsonPropertyName("data")]
    public List<CurseForgeVersionDataObj> Data { get; set; }
}

/// <summary>
/// CF 版本类型
/// </summary>
public record CurseForgeVersionTypeObj
{
    public record CurseForgeVersionTypeDataObj
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        //public int gameId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        //public string slug { get; set; }
    }

    [JsonPropertyName("data")]
    public List<CurseForgeVersionTypeDataObj> Data { get; set; }
}