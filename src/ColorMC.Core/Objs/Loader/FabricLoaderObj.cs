using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Loader;

public record FabricLoaderObj
{
    public record ArgumentsObj
    {
        [JsonPropertyName("game")]
        public List<string> Game { get; set; }
        [JsonPropertyName("jvm")]
        public List<string> Jvm { get; set; }
    }
    public record LibrariesObj
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    //public string inheritsFrom { get; set; }
    //public string releaseTime { get; set; }
    //public string time { get; set; }
    //public string type { get; set; }
    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; }
    [JsonPropertyName("arguments")]
    public ArgumentsObj Arguments { get; set; }
    [JsonPropertyName("libraries")]
    public List<LibrariesObj> Libraries { get; set; }
}

public record FabricLoaderVersionObj
{
    public record LoaderObj
    {
        //public string separator { get; set; }
        //public int build { get; set; }
        //public string maven { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        //public bool stable { get; set; }
    }
    //public record Intermediary
    //{
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //    public bool stable { get; set; }
    //}
    [JsonPropertyName("loader")]
    public LoaderObj Loader { get; set; }
    //public Intermediary intermediary { get; set; }
}

