using Newtonsoft.Json;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF MOD信息
/// </summary>
public record CurseForgeModObj
{
    public record DataObj
    {
        public record HashesObj
        {
            [JsonProperty("value")]
            public string Value { get; set; }
            [JsonProperty("algo")]
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
            [JsonProperty("modId")]
            public long ModId { get; set; }
            [JsonProperty("relationType")]
            public int RelationType { get; set; }
        }
        //public record Modules
        //{
        //    public string name { get; set; }
        //    public long fingerprint { get; set; }
        //}
        [JsonProperty("id")]
        public int Id { get; set; }
        //public int gameId { get; set; }
        [JsonProperty("modId")]
        public int ModId { get; set; }
        //public bool isAvailable { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        //public int releaseType { get; set; }
        //public int fileStatus { get; set; }
        [JsonProperty("hashes")]
        public List<HashesObj> Hashes { get; set; }
        [JsonProperty("fileDate")]
        public string FileDate { get; set; }
        [JsonProperty("fileLength")]
        public long FileLength { get; set; }
        [JsonProperty("downloadCount")]
        public long DownloadCount { get; set; }
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }
        //public List<string> gameVersions { get; set; }
        //public List<SortableGameVersions> sortableGameVersions { get; set; }
        [JsonProperty("dependencies")]
        public List<DependenciesObj> Dependencies { get; set; }
        //public int alternateFileId { get; set; }
        //public bool isServerPack { get; set; }
        //public long fileFingerprint { get; set; }
        //public List<Modules> modules { get; set; }
    }
    [JsonProperty("data")]
    public DataObj Data { get; set; }
}
