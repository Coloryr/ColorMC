using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Java;

public record OpenJ9Obj
{
    public record ResultsObj
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("pagepost_custom_js_value")]
        public string PagepostCustomJsValue { get; set; }
    }
    //public int code { get; set; }
    [JsonPropertyName("error")]
    public bool Error { get; set; }
    [JsonPropertyName("results")]
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
                [JsonPropertyName("downloadLink")]
                public string DownloadLink { get; set; }
                [JsonPropertyName("checksum")]
                public string Checksum { get; set; }
                //public string sig { get; set; }
            }
            [JsonPropertyName("opt1")]
            public OptObj Opt1 { get; set; }
        }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("version")]
        public int Version { get; set; }
        [JsonPropertyName("os")]
        public string Os { get; set; }
        [JsonPropertyName("arch")]
        public string Arch { get; set; }
        [JsonPropertyName("jdk")]
        public JdkObj Jdk { get; set; }
        [JsonPropertyName("jre")]
        public JdkObj Jre { get; set; }
    }

    [JsonPropertyName("downloads")]
    public List<Download> Downloads { get; set; }
}