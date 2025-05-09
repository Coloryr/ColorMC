using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CurseForge分类数据
/// </summary>
public record CurseForgeCategoriesObj
{
    public record CurseForgeCategoriesDataObj
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        //public int gameId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        //public string slug { get; set; }
        //public string url { get; set; }
        //public string iconUrl { get; set; }
        //public string dateModified { get; set; }
        [JsonPropertyName("classId")]
        public int ClassId { get; set; }
        //public int parentCategoryId { get; set; }
        //public int displayIndex { get; set; }
    }
    [JsonPropertyName("data")]
    public List<CurseForgeCategoriesDataObj> Data { get; set; }
}
