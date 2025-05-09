using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.MinecraftAPI;

/// <summary>
/// 皮肤信息
/// </summary>
/// <value></value>
public record UserProfileObj
{
    public record UserProfilePropertiesObj
    {
        //public string name { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        //public string signature { get; set; }
    }
    //public string id { get; set; }
    //public string name { get; set; }
    [JsonPropertyName("properties")]
    public List<UserProfilePropertiesObj> Properties { get; set; }
}
