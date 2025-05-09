using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF文件列表
/// </summary>
public record CurseForgeFileObj
{
    public record CurseForgeFilePaginationObj
    {
        //public int index { get; set; }
        //public int pageSize { get; set; }
        //public int resultCount { get; set; }
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }

    [JsonPropertyName("data")]
    public List<CurseForgeModObj.CurseForgeDataObj> Data { get; set; }

    [JsonPropertyName("pagination")]
    public CurseForgeFilePaginationObj Pagination { get; set; }
}
