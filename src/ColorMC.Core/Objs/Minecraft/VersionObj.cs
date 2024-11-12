using Newtonsoft.Json;

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
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        //public string time { get; set; }
        //public string releaseTime { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }
        //public int complianceLevel { get; set; }
    }

    //public Lastest lastest { get; set; }
    [JsonProperty("versions")]
    public List<VersionsObj> Versions { get; set; }
}
