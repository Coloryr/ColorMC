using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Java;

public record OpenJ9Obj
{
    public record ResultsObj
    {
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("pagepost_custom_js_value")]
        public string PagepostCustomJsValue { get; set; }
    }
    //public int code { get; set; }
    [JsonProperty("error")]
    public bool Error { get; set; }
    [JsonProperty("results")]
    public List<ResultsObj> Results { get; set; }
}

public record OpenJ9FileObj
{
    public record Download
    {
        public record JdkObj
        {
            public record OptObj
            {
                //public string displayName { get; set; }
                [JsonProperty("downloadLink")]
                public string DownloadLink { get; set; }
                [JsonProperty("checksum")]
                public string Checksum { get; set; }
                //public string sig { get; set; }
            }
            [JsonProperty("opt1")]
            public OptObj Opt1 { get; set; }
        }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("os")]
        public string Os { get; set; }
        [JsonProperty("arch")]
        public string Arch { get; set; }
        [JsonProperty("jdk")]
        public JdkObj Jdk { get; set; }
        [JsonProperty("jre")]
        public JdkObj Jre { get; set; }
    }

    [JsonProperty("downloads")]
    public List<Download> Downloads { get; set; }
}