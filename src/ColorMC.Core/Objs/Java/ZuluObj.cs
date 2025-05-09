using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Java;

public record ZuluObj
{
    //public string abi { get; set; }
    [JsonPropertyName("arch")]
    public string Arch { get; set; }
    //public string bundle_type { get; set; }
    //public List<object> cpu_gen { get; set; }
    //public string ext { get; set; }
    //public List<string> features { get; set; }
    [JsonPropertyName("hw_bitness")]
    public string HwBitness { get; set; }
    //public int id { get; set; }
    [JsonPropertyName("java_version")]
    public List<int> JavaVersion { get; set; }
    //public bool javafx { get; set; }
    //public List<int> jdk_version { get; set; }
    //public bool latest { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    //public int? openjdk_build_number { get; set; }
    [JsonPropertyName("os")]
    public string Os { get; set; }
    //public string release_status { get; set; }
    [JsonPropertyName("sha256_hash")]
    public string Sha256Hash { get; set; }
    //public string support_term { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("zulu_version")]
    public List<int> ZuluVersion { get; set; }
}
