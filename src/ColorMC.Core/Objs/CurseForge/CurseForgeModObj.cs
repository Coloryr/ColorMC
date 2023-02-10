namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF MOD信息
/// </summary>
public record CurseForgeModObj1
{
    public string Name { get; set; }
    public string File { get; set; }
    public string SHA1 { get; set; }
    public long Id { get; set; }
    public long ModId { get; set; }
    public string Url { get; set; }
}

/// <summary>
/// CF MOD信息
/// </summary>
public record CurseForgeModObj
{
    public record Data
    {
        public record Hashes
        {
            public string value { get; set; }
            public int algo { get; set; }
        }
        public record SortableGameVersions
        {
            public string gameVersionName { get; set; }
            public string gameVersionPadded { get; set; }
            public string gameVersion { get; set; }
            public string gameVersionReleaseDate { get; set; }
            public long? gameVersionTypeId { get; set; }
        }
        public record Dependencies
        {
            public long modId { get; set; }
            public int relationType { get; set; }
        }
        public record Modules
        {
            public string name { get; set; }
            public long fingerprint { get; set; }
        }
        public int id { get; set; }
        public int gameId { get; set; }
        public int modId { get; set; }
        public bool isAvailable { get; set; }
        public string displayName { get; set; }
        public string fileName { get; set; }
        public int releaseType { get; set; }
        public int fileStatus { get; set; }
        public List<Hashes> hashes { get; set; }
        public string fileDate { get; set; }
        public long fileLength { get; set; }
        public long downloadCount { get; set; }
        public string downloadUrl { get; set; }
        public List<string> gameVersions { get; set; }
        public List<SortableGameVersions> sortableGameVersions { get; set; }
        public List<Dependencies> dependencies { get; set; }
        public int alternateFileId { get; set; }
        public bool isServerPack { get; set; }
        public long fileFingerprint { get; set; }
        public List<Modules> modules { get; set; }
    }
    public Data data { get; set; }
}
