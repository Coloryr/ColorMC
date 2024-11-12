using Newtonsoft.Json;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CurseForge分类数据
/// </summary>
public record CurseForgeCategoriesObj
{
    public record DataObj
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        //public int gameId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        //public string slug { get; set; }
        //public string url { get; set; }
        //public string iconUrl { get; set; }
        //public string dateModified { get; set; }
        [JsonProperty("classId")]
        public int ClassId { get; set; }
        //public int parentCategoryId { get; set; }
        //public int displayIndex { get; set; }
    }
    [JsonProperty("data")]
    public List<DataObj> Data { get; set; }
}
