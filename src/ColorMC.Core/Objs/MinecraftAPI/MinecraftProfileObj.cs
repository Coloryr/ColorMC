using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.MinecraftAPI;

/// <summary>
/// 账户信息
/// </summary>
/// <value></value>
public record MinecraftProfileObj
{
    //public record MinecraftSkin
    //{
    //    public string id { get; set; }
    //    public string state { get; set; }
    //    public string url { get; set; }
    //    public string variant { get; set; }
    //    public string alias { get; set; }
    //}

    //public record Capes
    //{
    //    public string id { get; set; }
    //    public string state { get; set; }
    //    public string url { get; set; }
    //    public string alias { get; set; }
    //}
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    //public List<MinecraftSkin> skins { get; set; }
    //public List<Capes> capes { get; set; }
}
