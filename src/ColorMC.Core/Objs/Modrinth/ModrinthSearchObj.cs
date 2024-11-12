using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthSearchObj
{
    public record HitObj
    {
        [JsonProperty("project_id")]
        public string ProjectId { get; set; }
        //public string project_type { get; set; }
        //public string slug { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        //public List<string> categories { get; set; }
        //public List<string> display_categories { get; set; }
        //public List<string> versions { get; set; }
        [JsonProperty("downloads")]
        public int Downloads { get; set; }
        //public int follows { get; set; }
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
        //public string date_created { get; set; }
        [JsonProperty("date_modified")]
        public string DateModified { get; set; }
        //public string latest_version { get; set; }
        //public string license { get; set; }
        //public string client_side { get; set; }
        //public string server_side { get; set; }
    }
    [JsonProperty("hits")]
    public List<HitObj> Hits { get; set; }
}
