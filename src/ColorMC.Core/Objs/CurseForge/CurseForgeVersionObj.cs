using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF 版本数据
/// </summary>
public record CurseForgeVersionObj
{
    public record DataObj
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
        [JsonPropertyName("versions")]
        public List<string> Versions { get; set; }
    }

    [JsonPropertyName("data")]
    public List<DataObj> Data { get; set; }
}

/// <summary>
/// CF 版本类型
/// </summary>
public record CurseForgeVersionTypeObj
{
    public record DataObj
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        //public int gameId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        //public string slug { get; set; }
    }

    [JsonPropertyName("data")]
    public List<DataObj> Data { get; set; }
}