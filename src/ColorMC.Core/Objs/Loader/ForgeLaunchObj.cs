using ColorMC.Core.Objs.Minecraft;
using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Loader;

/// <summary>
/// Forge启动数据
/// </summary>
public record ForgeLaunchObj
{
    //public record Logging
    //{

    //}
    public record LibrariesObj
    {
        public record DownloadsObj
        {
            [JsonProperty("artifact")]
            public GameArgObj.LibrariesObj.DownloadsObj.ArtifactObj Artifact { get; set; }
        }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("downloads")]
        public DownloadsObj Downloads { get; set; }
    }
    public record ArgumentsObj
    {
        [JsonProperty("game")]
        public List<string> Game { get; set; }
        [JsonProperty("jvm")]
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
    [JsonProperty("mainClass")]
    public string MainClass { get; set; }
    //[JsonProperty("inheritsFrom")]
    //public string InheritsFrom { get; set; }
    //public Logging logging { get; set; }
    [JsonProperty("minecraftArguments")]
    public string MinecraftArguments { get; set; }
    [JsonProperty("arguments")]
    public ArgumentsObj Arguments { get; set; }
    [JsonProperty("libraries")]
    public List<LibrariesObj> Libraries { get; set; }
}
