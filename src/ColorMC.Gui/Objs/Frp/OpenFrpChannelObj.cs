using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpChannelObj
{
    public record ProxieObj
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("remote")]
        public string Remote { get; set; }
        //[JsonPropertyName("local")]
        //public string Local { get; set; }
    }
    public record OpenFrpChannelData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("proxies")]
        public List<ProxieObj> Proxies { get; set; }
    }
    //[JsonPropertyName("status")]
    //public int Status { get; set; }
    //[JsonPropertyName("success")]
    //public bool Success { get; set; }
    //[JsonPropertyName("message")]
    //public string Message { get; set; }
    [JsonPropertyName("data")]
    public List<OpenFrpChannelData> Data { get; set; }
}
