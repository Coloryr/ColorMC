using Newtonsoft.Json;

namespace ColorMC.Core.Objs.OptiFine;

public record OptifineListObj
{
    //public string _id { get; set; }
    [JsonProperty("mcversion")]
    public string Mcversion { get; set; }
    [JsonProperty("patch")]
    public string Patch { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("filename")]
    public string Filename { get; set; }
    [JsonProperty("forge")]
    public string Forge { get; set; }
}
