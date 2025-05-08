using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.OptiFine;

public record OptifineListObj
{
    //public string _id { get; set; }
    [JsonPropertyName("mcversion")]
    public string Mcversion { get; set; }
    [JsonPropertyName("patch")]
    public string Patch { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("filename")]
    public string Filename { get; set; }
    [JsonPropertyName("forge")]
    public string Forge { get; set; }
}
