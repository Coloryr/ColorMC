using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Loader;

public record QuiltMetaObj
{
    public record QuiltGameObj
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        //public bool stable { get; set; }
    }
    //public record Mappings
    //{
    //    public string gameVersion { get; set; }
    //    public string separator { get; set; }
    //    public int build { get; set; }
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //    public string hashed { get; set; }
    //}
    //public record Hashed
    //{
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //}
    public record QuiltMetaLoaderObj
    {
        //public string separator { get; set; }
        //public int build { get; set; }
        //public string maven { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
    //public record Installer
    //{
    //    public string url { get; set; }
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //}
    //public List<Game> game { get; set; }
    //public List<Mappings> mappings { get; set; }
    //public List<Hashed> hashed { get; set; }
    [JsonPropertyName("loader")]
    public List<QuiltMetaLoaderObj> Loader { get; set; }
    //public List<Installer> installer { get; set; }
}
