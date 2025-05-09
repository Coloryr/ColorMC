using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Java;

public record DragonwellObj
{
    public record ItemObj
    {
        [JsonPropertyName("version8")]
        public string Version8 { get; set; }
        [JsonPropertyName("xurl8")]
        public string Xurl8 { get; set; }
        [JsonPropertyName("aurl8")]
        public string Aurl8 { get; set; }
        [JsonPropertyName("wurl8")]
        public string Wurl8 { get; set; }
        //public string type8 { get; set; }
        [JsonPropertyName("version11")]
        public string Version11 { get; set; }
        [JsonPropertyName("xurl11")]
        public string Xurl11 { get; set; }
        [JsonPropertyName("aurl11")]
        public string Aurl11 { get; set; }
        [JsonPropertyName("apurl11")]
        public string Apurl11 { get; set; }
        [JsonPropertyName("wurl11")]
        public string Wurl11 { get; set; }
        [JsonPropertyName("rurl11")]
        public string Rurl11 { get; set; }
        //public string type11 { get; set; }
        [JsonPropertyName("version17")]
        public string Version17 { get; set; }
        [JsonPropertyName("xurl17")]
        public string Xurl17 { get; set; }
        [JsonPropertyName("aurl17")]
        public string Aurl17 { get; set; }
        [JsonPropertyName("apurl17")]
        public string Apurl17 { get; set; }
        [JsonPropertyName("wurl17")]
        public string Wurl17 { get; set; }
        //public string type17 { get; set; }
    }

    [JsonPropertyName("extended")]
    public ItemObj Extended { get; set; }
    [JsonPropertyName("standard")]
    public ItemObj Standard { get; set; }
}
