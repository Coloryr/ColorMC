using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Loader;

/// <summary>
/// 加载器数据
/// </summary>
public record QuiltLoaderObj
{
    public record QuiltArgumentsObj
    {
        [JsonPropertyName("game")]
        public List<string> Game { get; set; }
    }
    public record QuiltLibrariesObj
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
    public QuiltArgumentsObj Arguments { get; set; }
    [JsonPropertyName("libraries")]
    public List<QuiltLibrariesObj> Libraries { get; set; }
}
