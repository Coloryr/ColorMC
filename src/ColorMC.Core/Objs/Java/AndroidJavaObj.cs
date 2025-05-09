using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Java;

public record AndroidJavaItemObj
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }
    [JsonPropertyName("size")]
    public string Size { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
}
#if Phone
public record AndroidJavaObj
{
    [JsonPropertyName("jre8")]
    public List<AndroidJavaItemObj> Jre8 { get; set; }
    [JsonPropertyName("jre17")]
    public List<AndroidJavaItemObj> Jre17 { get; set; }
    [JsonPropertyName("jre21")]
    public List<AndroidJavaItemObj> Jre21 { get; set; }
}
#endif