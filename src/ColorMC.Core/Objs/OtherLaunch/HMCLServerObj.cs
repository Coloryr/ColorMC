using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.OtherLaunch;

public record HMCLServerObj
{
    public record HMCLServerFileObj
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }
    public record HMCLServerAddonsObj
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("author")]
    public string Author { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("fileApi")]
    public string FileApi { get; set; }
    [JsonPropertyName("files")]
    public List<HMCLServerFileObj> Files { get; set; }
    [JsonPropertyName("addons")]
    public List<HMCLServerAddonsObj> Addons { get; set; }
}
