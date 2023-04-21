using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Minecraft;

public record ModObj
{
    public string? modid { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    public string mcversion { get; set; }
    public string? version { get; set; }
    public List<string>? authorList { get; set; }
    public string credits { get; set; }
    public List<string?> dependants { get; set; }
    public List<string?> dependencies { get; set; }
    public string parent { get; set; }
    public string logoFile { get; set; }
    public List<string> screenshots { get; set; }
    public string updateUrl { get; set; }
    public string? url { get; set; }

    [JsonIgnore]
    public string Local { get; set; }
    [JsonIgnore]
    public bool Disable { get; set; }
    [JsonIgnore]
    public Loaders Loader { get; set; }
    [JsonIgnore]
    public bool V2 { get; set; }
    [JsonIgnore]
    public bool Broken { get; set; }
    [JsonIgnore]
    public string Sha1 { get; set; }
    [JsonIgnore]
    public GameSettingObj Game { get; set; }
}