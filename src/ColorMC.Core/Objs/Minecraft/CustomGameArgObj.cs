using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 自定义游戏启动配置
/// </summary>
public record CustomGameArgObj : GameArgObj
{
    [JsonProperty("compatibleJavaMajors")]
    public List<int> CompatibleJavaMajors { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("order")]
    public int Order { get; set; }
    [JsonProperty("uid")]
    public string Uid { get; set; }
    [JsonProperty("+tweakers")]
    public List<string> AddTweakers { get; set; }
    [JsonProperty("+jvmArgs")]
    public List<string> AddJvmArgs { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
    [JsonProperty("mainJar")]
    public LibrariesObj MainJar { get; set; }
    [JsonProperty("_minecraftVersion")]
    public string MinecraftVersion { get; set; }

    [JsonIgnore]
    public string File { get; set; }
}
