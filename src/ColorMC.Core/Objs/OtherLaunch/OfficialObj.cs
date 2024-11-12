using Newtonsoft.Json;

namespace ColorMC.Core.Objs.OtherLaunch;

public record OfficialObj
{
    public record PatchObj
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }
    public record LibrarieObj
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public record ArgumentsObj
    {
        [JsonProperty("game")]
        public List<object> Game { get; set; }
    }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("inheritsFrom")]
    public string InheritsFrom { get; set; }
    [JsonProperty("patches")]
    public List<PatchObj> Patches { get; set; }
    [JsonProperty("libraries")]
    public List<LibrarieObj> Libraries { get; set; }
    [JsonProperty("arguments")]
    public ArgumentsObj Arguments { get; set; }
}
