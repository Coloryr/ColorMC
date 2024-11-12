using Newtonsoft.Json;

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
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("primary")]
            public bool Primary { get; set; }
        }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("modLoaders")]
        public List<ModLoadersObj> ModLoaders { get; set; }
    }
    public record FilesObj
    {
        [JsonProperty("projectID")]
        public int ProjectID { get; set; }
        [JsonProperty("fileID")]
        public int FileID { get; set; }
        [JsonProperty("required")]
        public bool Required { get; set; }
    }
    [JsonProperty("minecraft")]
    public MinecraftObj Minecraft { get; set; }
    [JsonProperty("manifestType")]
    public string ManifestType { get; set; }
    [JsonProperty("manifestVersion")]
    public int ManifestVersion { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
    [JsonProperty("author")]
    public string Author { get; set; }
    [JsonProperty("files")]
    public List<FilesObj> Files { get; set; }
    [JsonProperty("overrides")]
    public string Overrides { get; set; }
}
