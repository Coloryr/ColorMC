using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 资源数据
/// </summary>
public record AssetsObj
{
    public record ObjectsObj
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
        //public long size { get; set; }
    }

    [JsonPropertyName("objects")]
    public Dictionary<string, ObjectsObj> Objects { get; set; }
}
