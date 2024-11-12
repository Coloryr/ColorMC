using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Java;

public record AndroidJavaItemObj
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("sha1")]
    public string Sha1 { get; set; }
    [JsonProperty("size")]
    public string Size { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
}

public record AndroidJavaObj
{
    [JsonProperty("jre8")]
    public List<AndroidJavaItemObj> Jre8 { get; set; }
    [JsonProperty("jre17")]
    public List<AndroidJavaItemObj> Jre17 { get; set; }
    [JsonProperty("jre21")]
    public List<AndroidJavaItemObj> Jre21 { get; set; }
}
