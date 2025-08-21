using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs;

public record ColorMcSaveObj
{
    [JsonPropertyName("type")]
    public SourceType Type { get; set; }
    [JsonPropertyName("pid")]
    public string PID { get; set; }
    [JsonPropertyName("fid")]
    public string FID { get; set; }
}
