using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthVersionObj
{
    public record FileObj
    {
        public record HasheObj
        {
            [JsonPropertyName("sha1")]
            public string Sha1 { get; set; }
            //public string sha512 { get; set; }
        }
        [JsonPropertyName("hashes")]
        public HasheObj Hashes { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        [JsonPropertyName("primary")]
        public bool Primary { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
    public record DependencieObj
    {
        [JsonPropertyName("version_id")]
        public string VersionId { get; set; }
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; }
        //public string file_name { get; set; }
        //public string dependency_type { get; set; }
    }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; }
    //public string iauthor_idd { get; set; }
    //public bool featured { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("version_number")]
    public string VersionNumber { get; set; }
    [JsonPropertyName("date_published")]
    public string DatePublished { get; set; }
    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }
    //public string version_type { get; set; }
    //public string status { get; set; }
    [JsonPropertyName("files")]
    public List<FileObj> Files { get; set; }
    [JsonPropertyName("dependencies")]
    public List<DependencieObj> Dependencies { get; set; }
}
