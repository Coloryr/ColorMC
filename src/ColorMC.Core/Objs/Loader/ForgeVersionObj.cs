using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Loader;

public record ForgeVersionBmclApiObj
{
    //public record Files
    //{
    //    public string format { get; set; }
    //    public string category { get; set; }
    //    public string hash { get; set; }
    //}
    //public int build { get; set; }
    //public List<Files> files { get; set; }
    //public string mcversion { get; set; }
    //public string modified { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; }
}