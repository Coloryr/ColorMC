using Newtonsoft.Json;

namespace ColorMC.Core.Objs.FTB;

public record FTBFilesObj
{
    public record Files
    {
        public record Curseforge
        {
            public int project { get; set; }
            public int file { get; set; }
        }
        public string version { get; set; }
        public string path { get; set; }
        public string url { get; set; }
        public List<string> mirrors { get; set; }
        public string sha1 { get; set; }
        public int size { get; set; }
        public List<object> tags { get; set; }
        public bool clientonly { get; set; }
        public bool serveronly { get; set; }
        public bool optional { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public long updated { get; set; }
        public Curseforge curseforge { get; set; }
    }
    public List<Files> files { get; set; }
    public int installs { get; set; }
    public int plays { get; set; }
    public long refreshed { get; set; }
    public string changelog { get; set; }
    public int parent { get; set; }
    public string notification { get; set; }
    public List<string> links { get; set; }
    public string status { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public long updated { get; set; }
    [JsonProperty("private")]
    public bool _private { get; set; }
}
