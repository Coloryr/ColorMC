using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF 数据
/// </summary>
public record CurseForgePackObj
{
    public record MinecraftObj
    {
        public record ModLoadersObj
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("primary")]
            public bool Primary { get; set; }
        }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("modLoaders")]
        public List<ModLoadersObj> ModLoaders { get; set; }
    }
    public record FilesObj
    {
        [JsonPropertyName("projectID")]
        public int ProjectID { get; set; }
        [JsonPropertyName("fileID")]
        public int FileID { get; set; }
        [JsonPropertyName("required")]
        public bool Required { get; set; }
    }
    [JsonPropertyName("minecraft")]
    public MinecraftObj Minecraft { get; set; }
    [JsonPropertyName("manifestType")]
    public string ManifestType { get; set; }
    [JsonPropertyName("manifestVersion")]
    public int ManifestVersion { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; }
    [JsonPropertyName("author")]
    public string Author { get; set; }
    [JsonPropertyName("files")]
    public List<FilesObj> Files { get; set; }
    [JsonPropertyName("overrides")]
    public string Overrides { get; set; }
}
