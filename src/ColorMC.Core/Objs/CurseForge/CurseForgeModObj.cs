using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF MOD信息
/// </summary>
public record CurseForgeModObj
{
    public record CurseForgeDataObj
    {
        public record HashesObj
        {
            [JsonPropertyName("value")]
            public string Value { get; set; }
            [JsonPropertyName("algo")]
            public int Algo { get; set; }
        }
        //public record SortableGameVersions
        //{
        //    public string gameVersionName { get; set; }
        //    public string gameVersionPadded { get; set; }
        //    public string gameVersion { get; set; }
        //    public string gameVersionReleaseDate { get; set; }
        //    public long? gameVersionTypeId { get; set; }
        //}
        public record DependenciesObj
        {
            [JsonPropertyName("modId")]
            public long ModId { get; set; }
            [JsonPropertyName("relationType")]
            public int RelationType { get; set; }
        }
        //public record Modules
        //{
        //    public string name { get; set; }
        //    public long fingerprint { get; set; }
        //}
        [JsonPropertyName("id")]
        public int Id { get; set; }
        //public int gameId { get; set; }
        [JsonPropertyName("modId")]
        public int ModId { get; set; }
        //public bool isAvailable { get; set; }
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
        //public int releaseType { get; set; }
        //public int fileStatus { get; set; }
        [JsonPropertyName("hashes")]
        public List<HashesObj> Hashes { get; set; }
        [JsonPropertyName("fileDate")]
        public string FileDate { get; set; }
        [JsonPropertyName("fileLength")]
        public long FileLength { get; set; }
        [JsonPropertyName("downloadCount")]
        public long DownloadCount { get; set; }
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }
        //public List<string> gameVersions { get; set; }
        //public List<SortableGameVersions> sortableGameVersions { get; set; }
        [JsonPropertyName("dependencies")]
        public List<DependenciesObj> Dependencies { get; set; }
        //public int alternateFileId { get; set; }
        //public bool isServerPack { get; set; }
        //public long fileFingerprint { get; set; }
        //public List<Modules> modules { get; set; }
    }
    [JsonPropertyName("data")]
    public CurseForgeDataObj Data { get; set; }
}
