using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 资源数据
/// </summary>
public record AssetsObj
{
    public record ObjectsObj
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }
        //public long size { get; set; }
    }

    [JsonProperty("objects")]
    public Dictionary<string, ObjectsObj> Objects { get; set; }
}
