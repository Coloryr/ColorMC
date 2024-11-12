using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthCategoriesObj
{
    //public string icon { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("project_type")]
    public string ProjectType { get; set; }
    [JsonProperty("header")]
    public string Header { get; set; }
}
