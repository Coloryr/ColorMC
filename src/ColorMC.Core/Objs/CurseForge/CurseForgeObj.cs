namespace ColorMC.Core.Objs.CurseForge;

public enum CurseForgeSortField
{
    Featured = 1,
    Popularity = 2,
    LastUpdated = 3,
    Name = 4,
    Author = 5,
    TotalDownloads = 6,
    Category = 7,
    GameVersion = 8
}

/// <summary>
/// CF 数据列表
/// </summary>
public record CurseForgeObj
{
    public record Data
    {
        public record Links
        {
            public string websiteUrl { get; set; }
            public string wikiUrl { get; set; }
            public string issuesUrl { get; set; }
            public string sourceUrl { get; set; }
        }
        public record Categories
        {
            public long id { get; set; }
            public long gameId { get; set; }
            public string name { get; set; }
            public string slug { get; set; }
            public string url { get; set; }
            public string iconUrl { get; set; }
            public string dateModified { get; set; }
            public bool isClass { get; set; }
            public long classId { get; set; }
            public long parentCategoryId { get; set; }
        }
        public record Authors
        {
            public long id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
        }
        public record Logo
        {
            public long id { get; set; }
            public long modId { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string thumbnailUrl { get; set; }
            public string url { get; set; }
        }
        public record Screenshots
        {
            public long id { get; set; }
            public long modId { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string thumbnailUrl { get; set; }
            public string url { get; set; }
        }
        public record LatestFiles
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
            public long id { get; set; }
            public long gameId { get; set; }
            public long modId { get; set; }
            public bool isAvailable { get; set; }
            public string displayName { get; set; }
            public string fileName { get; set; }
            public long releaseType { get; set; }
            public long fileStatus { get; set; }
            public List<Hashes> hashes { get; set; }
            public string fileDate { get; set; }
            public long fileLength { get; set; }
            public long downloadCount { get; set; }
            public string downloadUrl { get; set; }
            public List<string> gameVersions { get; set; }
            public List<SortableGameVersions> sortableGameVersions { get; set; }
            public List<Dependencies> dependencies { get; set; }
            public long alternateFileId { get; set; }
            public bool isServerPack { get; set; }
            public long fileFingerprint { get; set; }
            public List<Modules> modules { get; set; }
        }
        public record LatestFilesIndexes
        {
            public string gameVersion { get; set; }
            public long fileId { get; set; }
            public string filename { get; set; }
            public long releaseType { get; set; }
            public long? gameVersionTypeId { get; set; }
            public object modLoader { get; set; }
        }
        public long id { get; set; }
        public long gameId { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public Links links { get; set; }
        public string summary { get; set; }
        public long status { get; set; }
        public long downloadCount { get; set; }
        public bool isFeatured { get; set; }
        public long primaryCategoryId { get; set; }
        public List<Categories> categories { get; set; }
        public long classId { get; set; }
        public List<Authors> authors { get; set; }
        public Logo logo { get; set; }
        public List<Screenshots> screenshots { get; set; }
        public long mainFileId { get; set; }
        public List<LatestFiles> latestFiles { get; set; }
        public List<LatestFilesIndexes> latestFilesIndexes { get; set; }
        public string dateCreated { get; set; }
        public string dateModified { get; set; }
        public string dateReleased { get; set; }
        public bool? allowModDistribution { get; set; }
        public long gamePopularityRank { get; set; }
        public bool isAvailable { get; set; }
        public long thumbsUpCount { get; set; }
    }
    public record Pagination
    {
        public int index { get; set; }
        public int pageSize { get; set; }
        public int resultCount { get; set; }
        public int totalCount { get; set; }
    }

    public List<Data> data;
    public Pagination pagination { get; set; }
}
