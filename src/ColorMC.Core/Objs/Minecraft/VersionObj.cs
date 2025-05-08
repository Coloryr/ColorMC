using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 版本数据
/// </summary>
/// <value></value>
public record VersionObj
{
    //public record Lastest
    //{
    //    public string release { get; set; }
    //    public string snapshot { get; set; }
    //}

    public record VersionsObj
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        //public string time { get; set; }
        //public string releaseTime { get; set; }
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
        //public int complianceLevel { get; set; }
    }

    //public Lastest lastest { get; set; }
    [JsonPropertyName("versions")]
    public List<VersionsObj> Versions { get; set; }
}
