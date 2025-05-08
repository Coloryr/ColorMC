using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthVersionFileObj
{
    public record FileObj
    {
        //public record HashesObj
        //{
        //    public string sha1 { get; set; }
        //    public string sha512 { get; set; }
        //}
        //public HashesObj hashes { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        //public object file_type { get; set; }
        //public bool primary { get; set; }
        //public long size { get; set; }
    }
    //public List<string> game_versions { get; set; }
    //public List<string> loaders { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; }
    //public string author_id { get; set; }
    //public bool featured { get; set; }
    //public string name { get; set; }
    //public string version_number { get; set; }
    //public string changelog { get; set; }
    //public string changelog_url { get; set; }
    //public string date_published { get; set; }
    //public int downloads { get; set; }
    //public string version_type { get; set; }
    //public string status { get; set; }
    //public object requested_status { get; set; }
    [JsonPropertyName("files")]
    public List<FileObj> Files { get; set; }
}
