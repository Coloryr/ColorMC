using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthPackObj
{
    public record FileObj
    {
        public record HashObj
        {
            [JsonProperty("sha1")]
            public string Sha1 { get; set; }
            [JsonProperty("sha512")]
            public string Sha512 { get; set; }
        }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("hashes")]
        public HashObj Hashes { get; set; }
        [JsonProperty("downloads")]
        public List<string> Downloads { get; set; }
        [JsonProperty("fileSize")]
        public long FileSize { get; set; }
    }
    [JsonProperty("formatVersion")]
    public int FormatVersion { get; set; }
    //public string game { get; set; }
    [JsonProperty("versionId")]
    public string VersionId { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("summary")]
    public string Summary { get; set; }
    [JsonProperty("files")]
    public List<FileObj> Files { get; set; }
    [JsonProperty("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; }
}
