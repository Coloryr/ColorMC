using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.OtherLaunch;

public record OfficialObj
{
    public record PatchObj
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
    public record LibrarieObj
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
    public record ArgumentsObj
    {
        [JsonPropertyName("game")]
        public List<object> Game { get; set; }
    }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("inheritsFrom")]
    public string InheritsFrom { get; set; }
    [JsonPropertyName("patches")]
    public List<PatchObj> Patches { get; set; }
    [JsonPropertyName("libraries")]
    public List<LibrarieObj> Libraries { get; set; }
    [JsonPropertyName("arguments")]
    public ArgumentsObj Arguments { get; set; }
}
