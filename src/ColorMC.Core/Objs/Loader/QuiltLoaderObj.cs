using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Loader;

/// <summary>
/// 加载器数据
/// </summary>
public record QuiltLoaderObj
{
    public record ArgumentsObj
    {
        [JsonProperty("game")]
        public List<string> Game { get; set; }
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
