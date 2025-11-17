using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthProjectObj
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    //public string slug { get; set; }
    [JsonPropertyName("project_type")]
    public string ProjectType { get; set; }
    //public string team { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
    //public string published { get; set; }
    [JsonPropertyName("updated")]
    public string Updated { get; set; }
    //public string approved { get; set; }
    //public string status { get; set; }
    //public string client_side { get; set; }
    //public string server_side { get; set; }
    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }
    //public int followers { get; set; }
    //public List<string> game_versions { get; set; }
    //public List<string> loaders { get; set; }
    //public List<string> versions { get; set; }
    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }
    //public string issues_url { get; set; }
    //public string source_url { get; set; }
    //public string wiki_url { get; set; }
    //public string discord_url { get; set; }
}
