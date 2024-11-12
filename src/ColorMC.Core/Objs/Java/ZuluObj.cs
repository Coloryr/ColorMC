using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Java;

public record ZuluObj
{
    //public string abi { get; set; }
    [JsonProperty("arch")]
    public string Arch { get; set; }
    //public string bundle_type { get; set; }
    //public List<object> cpu_gen { get; set; }
    //public string ext { get; set; }
    //public List<string> features { get; set; }
    [JsonProperty("hw_bitness")]
    public string HwBitness { get; set; }
    //public int id { get; set; }
    [JsonProperty("java_version")]
    public List<int> JavaVersion { get; set; }
    //public bool javafx { get; set; }
    //public List<int> jdk_version { get; set; }
    //public bool latest { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    //public int? openjdk_build_number { get; set; }
    [JsonProperty("os")]
    public string Os { get; set; }
    //public string release_status { get; set; }
    [JsonProperty("sha256_hash")]
    public string Sha256Hash { get; set; }
    //public string support_term { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("zulu_version")]
    public List<int> ZuluVersion { get; set; }
}
