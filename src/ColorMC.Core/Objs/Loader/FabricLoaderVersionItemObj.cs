using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Loader;

public record FabricLoaderObj
{
    public record FabricArgumentsObj
    {
        [JsonPropertyName("game")]
        public List<string> Game { get; set; }
        [JsonPropertyName("jvm")]
        public List<string> Jvm { get; set; }
    }
    public record FabricLibrariesObj
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
    public FabricArgumentsObj Arguments { get; set; }
    [JsonPropertyName("libraries")]
    public List<FabricLibrariesObj> Libraries { get; set; }
}

public record FabricLoaderVersionObj
{
    public record FabricLoaderVersionItemObj
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
    public FabricLoaderVersionItemObj Loader { get; set; }
    //public Intermediary intermediary { get; set; }
}

