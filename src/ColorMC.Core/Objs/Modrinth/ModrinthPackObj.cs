using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthPackObj
{
    public record FileObj
    {
        public record HashObj
        {
            [JsonPropertyName("sha1")]
            public string Sha1 { get; set; }
            [JsonPropertyName("sha512")]
            public string Sha512 { get; set; }
        }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("hashes")]
        public HashObj Hashes { get; set; }
        [JsonPropertyName("downloads")]
        public List<string> Downloads { get; set; }
        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }
    }
    [JsonPropertyName("formatVersion")]
    public int FormatVersion { get; set; }
    //public string game { get; set; }
    [JsonPropertyName("versionId")]
    public string VersionId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("summary")]
    public string Summary { get; set; }
    [JsonPropertyName("files")]
    public List<FileObj> Files { get; set; }
    [JsonPropertyName("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; }
}
