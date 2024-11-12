using Newtonsoft.Json;

namespace ColorMC.Core.Objs.MinecraftAPI;

/// <summary>
/// 皮肤信息
/// </summary>
/// <value></value>
public record UserProfileObj
{
    public record PropertiesObj
    {
        //public string name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        //public string signature { get; set; }
    }
    //public string id { get; set; }
    //public string name { get; set; }
    [JsonProperty("properties")]
    public List<PropertiesObj> Properties { get; set; }
}
