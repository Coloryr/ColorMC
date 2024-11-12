using Newtonsoft.Json;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF 版本数据
/// </summary>
public record CurseForgeVersion
{
    public record DataObj
    {
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("versions")]
        public List<string> Versions { get; set; }
    }

    [JsonProperty("data")]
    public List<DataObj> Data { get; set; }
}

/// <summary>
/// CF 版本类型
/// </summary>
public record CurseForgeVersionType
{
    public record DataObj
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        //public int gameId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        //public string slug { get; set; }
    }

    [JsonProperty("data")]
    public List<DataObj> Data { get; set; }
}