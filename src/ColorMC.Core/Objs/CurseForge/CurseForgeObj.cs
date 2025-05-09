using System.Text.Json.Serialization;

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

public record CurseForgeObj
{
    [JsonPropertyName("data")]
    public CurseForgeListObj.CurseForgeListDataObj Data { get; set; }
}

public record CurseForgeModsInfoObj
{
    [JsonPropertyName("modIds")]
    public List<long> ModIds { get; set; }
    [JsonPropertyName("filterPcOnly")]
    public bool FilterPcOnly { get; set; }
}

/// <summary>
/// CF 数据列表
/// </summary>
public record CurseForgeListObj
{
    public record CurseForgeListDataObj
    {
        public record LinksObj
        {
            [JsonPropertyName("websiteUrl")]
            public string WebsiteUrl { get; set; }
            //public string wikiUrl { get; set; }
            //public string issuesUrl { get; set; }
            //public string sourceUrl { get; set; }
        }
        public record CategoriesObj
        {
            //public long id { get; set; }
            //public long gameId { get; set; }
            //public string name { get; set; }
            //public string slug { get; set; }
            //public string url { get; set; }
            //public string iconUrl { get; set; }
            //public string dateModified { get; set; }
            //public bool isClass { get; set; }
            [JsonPropertyName("classId")]
            public long ClassId { get; set; }
            //public long parentCategoryId { get; set; }
        }
        public record AuthorsObj
        {
            //public long id { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            //public string url { get; set; }
        }
        public record LogoObj
        {
            //public long id { get; set; }
            //public long modId { get; set; }
            //public string title { get; set; }
            //public string description { get; set; }
            //public string thumbnailUrl { get; set; }
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
        //public record Screenshots
        //{
        //    public long id { get; set; }
        //    public long modId { get; set; }
        //    public string title { get; set; }
        //    public string description { get; set; }
        //    public string thumbnailUrl { get; set; }
        //    public string url { get; set; }
        //}
        //public record LatestFilesIndexes
        //{
        //    public string gameVersion { get; set; }
        //    public long fileId { get; set; }
        //    public string filename { get; set; }
        //    public long releaseType { get; set; }
        //    public long? gameVersionTypeId { get; set; }
        //    public object modLoader { get; set; }
        //}
        [JsonPropertyName("id")]
        public long Id { get; set; }
        //public long gameId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        //public string slug { get; set; }
        [JsonPropertyName("links")]
        public LinksObj Links { get; set; }
        [JsonPropertyName("summary")]
        public string Summary { get; set; }
        //public long status { get; set; }
        [JsonPropertyName("downloadCount")]
        public long DownloadCount { get; set; }
        //public bool isFeatured { get; set; }
        //public long primaryCategoryId { get; set; }
        [JsonPropertyName("categories")]
        public List<CategoriesObj> Categories { get; set; }
        [JsonPropertyName("classId")]
        public long ClassId { get; set; }
        [JsonPropertyName("authors")]
        public List<AuthorsObj> Authors { get; set; }
        [JsonPropertyName("logo")]
        public LogoObj Logo { get; set; }
        //public List<Screenshots> screenshots { get; set; }
        //public long mainFileId { get; set; }
        //public List<CurseForgeModObj.DataObj> latestFiles { get; set; }
        //public List<LatestFilesIndexes> latestFilesIndexes { get; set; }
        //public string dateCreated { get; set; }
        [JsonPropertyName("dateModified")]
        public string DateModified { get; set; }
        //public string dateReleased { get; set; }
        //public bool? allowModDistribution { get; set; }
        //public long gamePopularityRank { get; set; }
        //public bool isAvailable { get; set; }
        //public long thumbsUpCount { get; set; }
    }
    public record CurseForgeListPaginationObj
    {
        //public int index { get; set; }
        //public int pageSize { get; set; }
        //public int resultCount { get; set; }
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }

    [JsonPropertyName("data")]
    public List<CurseForgeListDataObj> Data { get; set; }
    [JsonPropertyName("pagination")]
    public CurseForgeListPaginationObj Pagination { get; set; }
}
