using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Java;

public record DragonwellObj
{
    public record ItemObj
    {
        [JsonProperty("version8")]
        public string Version8 { get; set; }
        [JsonProperty("xurl8")]
        public string Xurl8 { get; set; }
        [JsonProperty("aurl8")]
        public string Aurl8 { get; set; }
        [JsonProperty("wurl8")]
        public string Wurl8 { get; set; }
        //public string type8 { get; set; }
        [JsonProperty("version11")]
        public string Version11 { get; set; }
        [JsonProperty("xurl11")]
        public string Xurl11 { get; set; }
        [JsonProperty("aurl11")]
        public string Aurl11 { get; set; }
        [JsonProperty("apurl11")]
        public string Apurl11 { get; set; }
        [JsonProperty("wurl11")]
        public string Wurl11 { get; set; }
        [JsonProperty("rurl11")]
        public string Rurl11 { get; set; }
        //public string type11 { get; set; }
        [JsonProperty("version17")]
        public string Version17 { get; set; }
        [JsonProperty("xurl17")]
        public string Xurl17 { get; set; }
        [JsonProperty("aurl17")]
        public string Aurl17 { get; set; }
        [JsonProperty("apurl17")]
        public string Apurl17 { get; set; }
        [JsonProperty("wurl17")]
        public string Wurl17 { get; set; }
        //public string type17 { get; set; }
    }

    [JsonProperty("extended")]
    public ItemObj Extended { get; set; }
    [JsonProperty("standard")]
    public ItemObj Standard { get; set; }
}
