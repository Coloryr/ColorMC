using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthSearchObj
{
    public record HitObj
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; }
        //public string project_type { get; set; }
        //public string slug { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        //public List<string> categories { get; set; }
        //public List<string> display_categories { get; set; }
        //public List<string> versions { get; set; }
        [JsonPropertyName("downloads")]
        public int Downloads { get; set; }
        //public int follows { get; set; }
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; }
        //public string date_created { get; set; }
        [JsonPropertyName("date_modified")]
        public string DateModified { get; set; }
        //public string latest_version { get; set; }
        //public string license { get; set; }
        //public string client_side { get; set; }
        //public string server_side { get; set; }
    }
    [JsonPropertyName("hits")]
    public List<HitObj> Hits { get; set; }
}
