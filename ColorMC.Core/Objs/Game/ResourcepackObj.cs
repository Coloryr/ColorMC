using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Game;

public record ResourcepackObj
{
    public record Pack
    {
        public string description { get; set; }
        public int pack_format { get; set; }
    }
    public Pack pack { get; set; }

    [JsonIgnore]
    public string Local { get; set; }
    [JsonIgnore]
    public byte[] Icon { get; set; }
    [JsonIgnore]
    public bool Disable { get; set; }
}
