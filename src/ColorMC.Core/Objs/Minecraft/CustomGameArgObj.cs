using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 自定义游戏启动配置
/// </summary>
public record CustomGameArgObj : GameArgObj
{
    [JsonPropertyName("compatibleJavaMajors")]
    public List<int> CompatibleJavaMajors { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("order")]
    public int Order { get; set; }
    [JsonPropertyName("uid")]
    public string Uid { get; set; }
    [JsonPropertyName("+tweakers")]
    public List<string> AddTweakers { get; set; }
    [JsonPropertyName("+jvmArgs")]
    public List<string> AddJvmArgs { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; }
    [JsonPropertyName("mainJar")]
    public LibrariesObj MainJar { get; set; }
    [JsonPropertyName("_minecraftVersion")]
    public string MinecraftVersion { get; set; }

    [JsonIgnore]
    public string File { get; set; }
}
