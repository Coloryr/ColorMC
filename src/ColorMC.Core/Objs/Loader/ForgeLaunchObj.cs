using System.Text.Json.Serialization;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs.Loader;

/// <summary>
/// Forge启动数据
/// </summary>
public record ForgeLaunchObj
{
    //public record Logging
    //{

    //}
    public record ForgeLibrariesObj
    {
        public record ForgeDownloadsObj
        {
            [JsonPropertyName("artifact")]
            public GameArgObj.GameLibrariesObj.GameLibrariesDownloadsObj.ArtifactObj Artifact { get; set; }
        }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("downloads")]
        public ForgeDownloadsObj Downloads { get; set; }
    }
    public record ForgeArgumentsObj
    {
        [JsonPropertyName("game")]
        public List<string> Game { get; set; }
        [JsonPropertyName("jvm")]
        public List<string> Jvm { get; set; }
    }
    //public List<string> _comment_ { get; set; }
    //[JsonProperty("id")]
    //public string Id { get; set; }
    //[JsonProperty("time")]
    //public string Time { get; set; }
    //[JsonProperty("releaseTime")]
    //public string ReleaseTime { get; set; }
    //[JsonProperty("type")]
    //public string Type { get; set; }
    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; }
    //[JsonProperty("inheritsFrom")]
    //public string InheritsFrom { get; set; }
    //public Logging logging { get; set; }
    [JsonPropertyName("minecraftArguments")]
    public string MinecraftArguments { get; set; }
    [JsonPropertyName("arguments")]
    public ForgeArgumentsObj Arguments { get; set; }
    [JsonPropertyName("libraries")]
    public List<ForgeLibrariesObj> Libraries { get; set; }
}
