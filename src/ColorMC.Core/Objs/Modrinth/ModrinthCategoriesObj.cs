using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthCategoriesObj
{
    //public string icon { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("project_type")]
    public string ProjectType { get; set; }
    [JsonPropertyName("header")]
    public string Header { get; set; }
}
