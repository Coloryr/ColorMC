using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Loader;

public record FabricLoaderObj
{
    public record ArgumentsObj
    {
        [JsonProperty("game")]
        public List<string> Game { get; set; }
        [JsonProperty("jvm")]
        public List<string> Jvm { get; set; }
    }
    public record LibrariesObj
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    [JsonProperty("id")]
    public string Id { get; set; }
    //public string inheritsFrom { get; set; }
    //public string releaseTime { get; set; }
    //public string time { get; set; }
    //public string type { get; set; }
    [JsonProperty("mainClass")]
    public string MainClass { get; set; }
    [JsonProperty("arguments")]
    public ArgumentsObj Arguments { get; set; }
    [JsonProperty("libraries")]
    public List<LibrariesObj> Libraries { get; set; }
}

public record FabricLoaderVersionObj
{
    public record LoaderObj
    {
        //public string separator { get; set; }
        //public int build { get; set; }
        //public string maven { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        //public bool stable { get; set; }
    }
    //public record Intermediary
    //{
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //    public bool stable { get; set; }
    //}
    [JsonProperty("loader")]
    public LoaderObj Loader { get; set; }
    //public Intermediary intermediary { get; set; }
}

