using Newtonsoft.Json;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF文件列表
/// </summary>
public record CurseForgeFileObj
{
    public record PaginationObj
    {
        //public int index { get; set; }
        //public int pageSize { get; set; }
        //public int resultCount { get; set; }
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
    }

    [JsonProperty("data")]
    public List<CurseForgeModObj.DataObj> Data { get; set; }

    [JsonProperty("pagination")]
    public PaginationObj Pagination { get; set; }
}
