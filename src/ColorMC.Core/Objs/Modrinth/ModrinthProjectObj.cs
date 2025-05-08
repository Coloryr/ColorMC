using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthProjectObj
{
    //public string id { get; set; }
    //public string slug { get; set; }
    //public string project_type { get; set; }
    //public string team { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    //public string description { get; set; }
    //public string published { get; set; }
    //public string updated { get; set; }
    //public string approved { get; set; }
    //public string status { get; set; }
    //public string client_side { get; set; }
    //public string server_side { get; set; }
    //public int downloads { get; set; }
    //public int followers { get; set; }
    //public List<string> game_versions { get; set; }
    //public List<string> loaders { get; set; }
    //public List<string> versions { get; set; }
    //public string icon_url { get; set; }
    //public string issues_url { get; set; }
    //public string source_url { get; set; }
    //public string wiki_url { get; set; }
    //public string discord_url { get; set; }
}
