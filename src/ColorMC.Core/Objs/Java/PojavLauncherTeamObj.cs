using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Java;

public record PojavLauncherTeamItem
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

public record PojavLauncherTeamObj
{
    [JsonProperty("jre8")]
    public List<PojavLauncherTeamItem> Jre8 { get; set; }
    [JsonProperty("jre17")]
    public List<PojavLauncherTeamItem> Jre17 { get; set; }
    [JsonProperty("jre21")]
    public List<PojavLauncherTeamItem> Jre21 { get; set; }
}
