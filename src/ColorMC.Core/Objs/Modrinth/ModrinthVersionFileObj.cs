using Newtonsoft.Json;

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
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("filename")]
        public string Filename { get; set; }
        //public object file_type { get; set; }
        //public bool primary { get; set; }
        //public long size { get; set; }
    }
    //public List<string> game_versions { get; set; }
    //public List<string> loaders { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("project_id")]
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
    [JsonProperty("files")]
    public List<FileObj> Files { get; set; }
}
