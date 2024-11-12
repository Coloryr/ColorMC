using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthVersionObj
{
    public record FileObj
    {
        public record HasheObj
        {
            [JsonProperty("sha1")]
            public string Sha1 { get; set; }
            //public string sha512 { get; set; }
        }
        [JsonProperty("hashes")]
        public HasheObj Hashes { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("filename")]
        public string Filename { get; set; }
        [JsonProperty("primary")]
        public bool Primary { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }
    }
    public record DependencieObj
    {
        [JsonProperty("version_id")]
        public string VersionId { get; set; }
        [JsonProperty("project_id")]
        public string ProjectId { get; set; }
        //public string file_name { get; set; }
        //public string dependency_type { get; set; }
    }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("project_id")]
    public string ProjectId { get; set; }
    //public string iauthor_idd { get; set; }
    //public bool featured { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("version_number")]
    public string VersionNumber { get; set; }
    [JsonProperty("date_published")]
    public string DatePublished { get; set; }
    [JsonProperty("downloads")]
    public int Downloads { get; set; }
    //public string version_type { get; set; }
    //public string status { get; set; }
    [JsonProperty("files")]
    public List<FileObj> Files { get; set; }
    [JsonProperty("dependencies")]
    public List<DependencieObj> Dependencies { get; set; }
}
