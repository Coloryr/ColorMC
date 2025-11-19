using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthCategoriesObj
{
    [JsonPropertyName("icon")]
    public string Icon { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("project_type")]
    public string ProjectType { get; set; }
    [JsonPropertyName("header")]
    public string Header { get; set; }
}
